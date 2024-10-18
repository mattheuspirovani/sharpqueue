using System.Net;
using System.Net.Sockets;

namespace SharpQueue.NotificationService;

public class NotificationServer
{
    private TcpListener _server;
    private readonly List<ClientConnectionHandler> _connectedClients;
    private readonly ConsumerSubscriptionManager _subscriptionManager;
    private readonly MessageProcessor _messageProcessor;

    public NotificationServer(string ipAddress, int port)
    {
        _server = new TcpListener(IPAddress.Parse(ipAddress), port);
        _connectedClients = new List<ClientConnectionHandler>();
        _subscriptionManager = new ConsumerSubscriptionManager();
        _messageProcessor = new MessageProcessor(_subscriptionManager);
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _server.Start();
        Console.WriteLine("Notification Server started...");

        while (!stoppingToken.IsCancellationRequested)
        {
            var client = await _server.AcceptTcpClientAsync();
            var clientHandler = new ClientConnectionHandler(client);
            _connectedClients.Add(clientHandler);
            Console.WriteLine("Client connected!");

            _ = Task.Run(() => HandleClient(clientHandler), stoppingToken);
        }
    }

    private async Task HandleClient(ClientConnectionHandler clientHandler)
    {
        while (clientHandler.IsConnected())
        {
            var message = await clientHandler.ReadMessageAsync();
            if (message != null)
            {
                await _messageProcessor.ProcessAsync(message, clientHandler);
            }
        }

        Console.WriteLine("Client disconnected.");
        _connectedClients.Remove(clientHandler);
        _subscriptionManager.UnsubscribeClientFromAll(clientHandler);
        clientHandler.Disconnect();
    }

    public async Task NotifyClients(string queueName, string message)
    {
        var subscribedClients = _subscriptionManager.GetSubscribedClients(queueName);

        foreach (var clientHandler in subscribedClients)
        {
            if (clientHandler.IsConnected())
            {
                await clientHandler.SendMessageAsync(message);
            }
        }
    }
}

