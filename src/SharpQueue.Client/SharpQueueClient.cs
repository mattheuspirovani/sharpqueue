using System.Text;
using Microsoft.Extensions.Logging;

namespace SharpQueue.Client;

public class SharpQueueClient : IDisposable
{
    private readonly ITcpClient _client;
    private Stream? _stream;
    private readonly ILogger<SharpQueueClient> _logger;

    private const string CommandSubscribe = "COMMAND: SUBSCRIBE\nQUEUE: ";
    private const string CommandUnsubscribe = "COMMAND: UNSUBSCRIBE\nQUEUE: ";
    private const string CommandSend = "COMMAND: SEND\nQUEUE: ";
    private const string CommandMessage = "\nMESSAGE: ";

    public SharpQueueClient(ITcpClient client, ILogger<SharpQueueClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task ConnectAsync(string ipAddress, int port)
    {
        try
        {
            _client.Connect(ipAddress, port);
            _stream = _client.GetStream();
            _logger.LogInformation("Connected to SharpQueue server at {IpAddress}:{Port}", ipAddress, port);
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to connect to SharpQueue server.");
            throw;
        }
    }

    public async Task SubscribeAsync(string queueName)
    {
        await SendCommandAsync($"{CommandSubscribe}{queueName}\n", "SUBSCRIBE", queueName);
    }

    public async Task UnsubscribeAsync(string queueName)
    {
        await SendCommandAsync($"{CommandUnsubscribe}{queueName}\n", "UNSUBSCRIBE", queueName);
    }

    public async Task SendMessageAsync(string queueName, string message)
    {
        var command = $"{CommandSend}{queueName}{CommandMessage}{message}\n";
        await SendCommandAsync(command, "SEND", queueName, message);
    }

    private async Task SendCommandAsync(string command, string commandType, string queueName, string? message = null)
    {
        if (_stream == null)
        {
            throw new InvalidOperationException("Client is not connected.");
        }

        try
        {
            var data = Encoding.UTF8.GetBytes(command);
            await _stream.WriteAsync(data, 0, data.Length);
            if (message == null)
            {
                _logger.LogInformation("{CommandType} command sent for queue: {QueueName}", commandType, queueName);
            }
            else
            {
                _logger.LogInformation("{CommandType} command sent for queue: {QueueName}, message: {Message}", commandType, queueName, message);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Error sending {CommandType} command", commandType);
        }
    }

    public async Task<string?> ReceiveMessageAsync()
    {
        if (_stream == null)
        {
            throw new InvalidOperationException("Client is not connected.");
        }

        byte[] buffer = new byte[1024];
        try
        {
            int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
            var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            _logger.LogInformation("Message received from server: {Message}", message);
            return message;
        }
        catch (Exception ex)
        {
            LogError(ex, "Error receiving message from server.");
            return null;
        }
    }

    private void LogError(Exception ex, string messageTemplate, params object[] args)
    {
        _logger.LogError(ex, messageTemplate, args);
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _client.Dispose();
        _logger.LogInformation("SharpQueueClient disposed.");
    }
}
