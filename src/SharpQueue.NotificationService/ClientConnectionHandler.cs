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
        await _stream.WriteAsync(messageBytes);
    }

    public async Task<string?> ReadMessageAsync()
    {
        List<byte> data = [];

        var buffer = new byte[1024];
        while (_stream.DataAvailable || data.Count == 0)
        {
            var bytesRead = await _stream.ReadAsync(buffer);

            if (bytesRead == 0)
            {
                break;
            }
            data.AddRange(buffer.Take(bytesRead));
        }
        if (data.Count > 0)
        {
            return Encoding.UTF8.GetString([.. data]);
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
