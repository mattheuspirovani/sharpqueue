using System.Net.Sockets;

namespace SharpQueue.Client;

public interface ITcpClient : IDisposable
{
    void Connect(string ipAddress, int port);
    Stream GetStream();
    bool Connected { get; }
}
