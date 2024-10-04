using System;
using System.Collections.Concurrent;
using SharpQueue.Interfaces;

namespace SharpQueue;

public class QueueManager : IQueueManager
{
    private readonly ConcurrentDictionary<string, MessageQueue> _queues;

    public QueueManager()
    {
        _queues = new ConcurrentDictionary<string, MessageQueue>();
    }

    private MessageQueue GetOrCreateQueue(string queueName)
    {
        return _queues.GetOrAdd(queueName, new MessageQueue());
    }

    public async Task AddMessageAsync(string queueName, string messageBody)
    {
        var queue = GetOrCreateQueue(queueName);
        var message = new Message(messageBody);
        await queue.AddMessageAsync(message);
        Console.WriteLine($"Message added to queue '{queueName}': {messageBody}");
    }
    
    public async Task<Message?> ConsumeMessageAsync(string queueName)
    {
        if (_queues.TryGetValue(queueName, out var queue))
        {
            var message = await queue.ConsumeMessageAsync();
            if (message != null)
            {
                Console.WriteLine($"Message consumed from queue '{queueName}': {message.Body}");
            }
            else
            {
                Console.WriteLine($"No messages available in queue '{queueName}'.");
            }
            return message;
        }
        else
        {
            Console.WriteLine($"Queue '{queueName}' does not exist.");
        }
        return null;
    }

    public int GetQueueLength(string queueName)
    {
        if (_queues.TryGetValue(queueName, out var queue))
        {
            return queue.GetQueueLength();
        }
        return 0;
    }

    public bool RemoveQueue(string queueName)
    {
        return _queues.TryRemove(queueName, out _);
    }
}
