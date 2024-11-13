using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using SharpQueue.Client;

namespace SharpQueue.Tests;

public class SharpQueueClientTests
{
    private readonly Mock<ILogger<SharpQueueClient>> _mockLogger;
    private readonly Mock<ITcpClient> _mockTcpClient;
    private readonly SharpQueueClient _client;
    private readonly MemoryStream _memoryStream;

    public SharpQueueClientTests()
    {
        _mockLogger = new Mock<ILogger<SharpQueueClient>>();
        _mockTcpClient = new Mock<ITcpClient>();
        _memoryStream = new MemoryStream();
        _mockTcpClient.Setup(c => c.GetStream()).Returns(_memoryStream);
        _client = new SharpQueueClient(_mockTcpClient.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldCreateSharpQueueClientInstance()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SharpQueueClient>>();
        var mockTcpClient = new Mock<ITcpClient>();

        // Act
        var client = new SharpQueueClient(mockTcpClient.Object, mockLogger.Object);

        // Assert
        Assert.NotNull(client);
        Assert.IsType<SharpQueueClient>(client);
    }

    [Fact]
    public async Task ConnectAsync_ShouldAttemptToConnect()
    {
        // Arrange
        var ipAddress = "127.0.0.1";
        var port = 5151;

        // Act
        await _client.ConnectAsync(ipAddress, port);

        // Assert
        _mockTcpClient.Verify(client => client.Connect(ipAddress, port), Times.Once);
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Connected to SharpQueue server")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldSendSubscribeCommand()
    {
        // Arrange
        await _client.ConnectAsync("127.0.0.1", 5151); 
        var queueName = "testQueue";

        // Act
        await _client.SubscribeAsync(queueName);

        // Assert
        _memoryStream.Position = 0; 
        using var reader = new StreamReader(_memoryStream, Encoding.UTF8);
        var sentCommand = await reader.ReadToEndAsync();

        Assert.Contains("COMMAND: SUBSCRIBE", sentCommand);
        Assert.Contains($"QUEUE: {queueName}", sentCommand);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldThrowExceptionIfNotConnected()
    {
        // Arrange

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _client.SubscribeAsync("testQueue"));
    }

    [Fact]
    public async Task UnsubscribeAsync_ShouldSendUnsubscribeCommand()
    {
        // Arrange
        await _client.ConnectAsync("127.0.0.1", 5151); 
        var queueName = "testQueue";

        // Act
        await _client.UnsubscribeAsync(queueName);

        // Assert
        _memoryStream.Position = 0;
        using var reader = new StreamReader(_memoryStream, Encoding.UTF8);
        var sentCommand = await reader.ReadToEndAsync();

        Assert.Contains("COMMAND: UNSUBSCRIBE", sentCommand);
        Assert.Contains($"QUEUE: {queueName}", sentCommand);
    }

    [Fact]
    public async Task UnsubscribeAsync_ShouldThrowExceptionIfNotConnected()
    {
        // Arrange:

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _client.UnsubscribeAsync("testQueue"));
    }

    [Fact]
    public async Task SendMessageAsync_ShouldSendMessageCommand()
    {
        // Arrange
        await _client.ConnectAsync("127.0.0.1", 5151); 
        var queueName = "testQueue";
        var message = "Hello, World!";

        // Act
        await _client.SendMessageAsync(queueName, message);

        // Assert
        _memoryStream.Position = 0;
        using var reader = new StreamReader(_memoryStream, Encoding.UTF8);
        var sentCommand = await reader.ReadToEndAsync();

        Assert.Contains("COMMAND: SEND", sentCommand);
        Assert.Contains($"QUEUE: {queueName}", sentCommand);
        Assert.Contains($"MESSAGE: {message}", sentCommand);
    }

    [Fact]
    public async Task ReceiveMessageAsync_ShouldReturnReceivedMessage()
    {
        // Arrange
        await _client.ConnectAsync("127.0.0.1", 5151); // Simula a conexão
        var message = "Hello from server!";
        var messageData = Encoding.UTF8.GetBytes(message);

        _memoryStream.Write(messageData, 0, messageData.Length);
        _memoryStream.Position = 0; 

        // Act
        var receivedMessage = await _client.ReceiveMessageAsync();

        // Assert
        Assert.Equal(message, receivedMessage);
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Message received from server")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}

