using SharpQueue.Core.Interfaces;

namespace SharpQueue.NotificationService;

public class QueueNotifier(NotificationServer notificationServer) : IQueueNotifier
{
    private readonly NotificationServer _notificationServer = notificationServer;

    public async Task OnMessageAdded(string queueName, string message)
    {
        await _notificationServer.NotifyClients(queueName, message);
        Console.WriteLine($"Notifying clients about new message in queue '{queueName}': {message}");
    }
}