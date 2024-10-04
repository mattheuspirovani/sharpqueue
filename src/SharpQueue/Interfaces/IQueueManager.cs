using System;

namespace SharpQueue.Interfaces;

public interface IQueueManager
{
    Task AddMessageAsync(string queueName, string messageBody);
    Task<Message?> ConsumeMessageAsync(string queueName);
    int GetQueueLength(string queueName);
    bool RemoveQueue(string queueName);
}
