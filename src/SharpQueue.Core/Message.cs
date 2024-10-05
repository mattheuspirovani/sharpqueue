namespace SharpQueue;

public class Message
{
    public Guid Id { get; }
    public byte[] Body { get; }
    public DateTime Timestamp { get; }

    public Message(byte[] body)
    {
        Id = Guid.NewGuid();
        Body = body;
        Timestamp = DateTime.UtcNow;
    }

    public Message(string body) : this(System.Text.Encoding.UTF8.GetBytes(body))
    {
    }

    public string GetBodyAsString()
    {
        return System.Text.Encoding.UTF8.GetString(Body);
    }
}
