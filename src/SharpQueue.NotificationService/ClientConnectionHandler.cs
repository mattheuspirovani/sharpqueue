using System.Net.Sockets;
using System.Text;

namespace SharpQueue.NotificationService;

public class ClientConnectionHandler
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;

    public ClientConnectionHandler(TcpClient client)
    {
        _client = client;
        _stream = _client.GetStream();
    }

    public async Task SendMessageAsync(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
    }

    public async Task<string?> ReadMessageAsync()
    {
        var buffer = new byte[1024];
        var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);

        if (bytesRead > 0)
        {
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        return null;
    }

    public void Disconnect()
    {
        _client.Close();
    }

    public bool IsConnected()
    {
        return _client.Connected;
    }    
}
