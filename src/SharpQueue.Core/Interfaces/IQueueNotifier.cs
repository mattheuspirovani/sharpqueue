namespace SharpQueue.Core.Interfaces;

public interface IQueueNotifier
{
    Task OnMessageAdded(string queueName, string message);
}
