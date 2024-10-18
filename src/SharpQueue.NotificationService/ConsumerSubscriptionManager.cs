using System.Collections.Concurrent;

namespace SharpQueue.NotificationService;

public class ConsumerSubscriptionManager
{
    private readonly ConcurrentDictionary<string, List<ClientConnectionHandler>> _subscriptions;

    public ConsumerSubscriptionManager()
    {
        _subscriptions = new ConcurrentDictionary<string, List<ClientConnectionHandler>>();
    }

    public void SubscribeClient(string queueName, ClientConnectionHandler client)
    {
        if (_subscriptions.ContainsKey(queueName))
        {
            _subscriptions[queueName].Add(client);
        }
        else
        {
            _subscriptions.TryAdd(queueName, new List<ClientConnectionHandler> { client });
        }

        Console.WriteLine($"Client subscribed to queue '{queueName}'");
    }

    public void UnsubscribeClient(string queueName, ClientConnectionHandler client)
    {
        if (_subscriptions.ContainsKey(queueName))
        {
            _subscriptions[queueName].Remove(client);
            Console.WriteLine($"Client unsubscribed from queue '{queueName}'");
        }
    }

    public void UnsubscribeClientFromAll(ClientConnectionHandler client)
    {
        foreach (var queue in _subscriptions.Keys)
        {
            UnsubscribeClient(queue, client);
        }
    }

    public List<ClientConnectionHandler> GetSubscribedClients(string queueName)
    {
        if (_subscriptions.ContainsKey(queueName))
        {
            return _subscriptions[queueName];
        }

        return new List<ClientConnectionHandler>();
    }

    public async Task NotifySubscribedClients(string queueName, string message)
    {
        var clients = GetSubscribedClients(queueName);

        foreach (var client in clients)
        {
            if (client.IsConnected())
            {
                await client.SendMessageAsync(message);
            }
            else
            {
                UnsubscribeClient(queueName, client);
            }
        }

        Console.WriteLine($"Notified {clients.Count} clients for queue '{queueName}'");
    }
}