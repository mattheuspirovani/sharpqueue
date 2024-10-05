using System.Collections.Concurrent;

namespace SharpQueue;

public class MessageQueue
{
    private readonly ConcurrentQueue<Message> _queue;

    public MessageQueue()
    {
        _queue = new ConcurrentQueue<Message>();
    }

    public async Task AddMessageAsync(Message message)
    {
        await Task.Run(() => _queue.Enqueue(message));
    }

    public async Task<Message?> ConsumeMessageAsync()
    {
        return await Task.Run(() =>
        {
            if (_queue.TryDequeue(out var message))
            {
                return message;
            }
            return null;
        });
    }

    public int GetQueueLength()
    {
        return _queue.Count;
    }

    public bool IsEmpty()
    {
        return _queue.IsEmpty;
    }
}
