using System.Collections.Concurrent;
using SharpQueue.Core.Interfaces;
using SharpQueue.Interfaces;

namespace SharpQueue;

public class QueueManager : IQueueManager
{
    private readonly ConcurrentDictionary<string, MessageQueue> _queues;
    private readonly IQueueNotifier _queueNotifier;
    public QueueManager(IQueueNotifier queueNotifier)
    {
        _queues = new ConcurrentDictionary<string, MessageQueue>();
        _queueNotifier = queueNotifier;
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
        await _queueNotifier.OnMessageAdded(queueName, messageBody);
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
