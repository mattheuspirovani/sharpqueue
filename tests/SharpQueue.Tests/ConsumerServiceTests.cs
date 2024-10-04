using Moq;
using SharpQueue.Interfaces;

namespace SharpQueue.Tests;

public class ConsumerServiceTests
{
    private readonly ConsumerService _consumerService;
    private readonly Mock<IQueueManager> _mockQueueManager;

    public ConsumerServiceTests()
    {
        // Mock de IQueueManager ao invés de QueueManager
        _mockQueueManager = new Mock<IQueueManager>();
        _consumerService = new ConsumerService(_mockQueueManager.Object);
    }

    [Fact]
    public async Task RegisterConsumerAsync_ShouldProcessMessage_WhenMessageIsAvailable()
    {
        // Arrange
        var message = new Message("Test Message");
        var queueName = "testQueue";
        bool messageProcessed = false;

        _mockQueueManager
            .Setup(qm => qm.ConsumeMessageAsync(queueName))
            .ReturnsAsync(message);

        using var cts = new CancellationTokenSource();

        // Act
        await _consumerService.RegisterConsumerAsync(queueName, async (msg) =>
        {
            Assert.Equal("Test Message", msg.GetBodyAsString());
            messageProcessed = true;
            cts.Cancel();
            await Task.CompletedTask;
        }, cts.Token);

        // Assert
        Assert.True(messageProcessed);
    }

    [Fact]
    public async Task RegisterConsumerAsync_ShouldHandleNoMessagesGracefully()
    {
        // Arrange
        var queueName = "emptyQueue";

        _mockQueueManager
            .Setup(qm => qm.ConsumeMessageAsync(queueName))
            .ReturnsAsync((Message?)null); // Simula uma fila sem mensagens

        bool messageProcessed = false;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act
        await _consumerService.RegisterConsumerAsync(queueName, async (msg) =>
        {
            messageProcessed = true;
            cts.Cancel();
            await Task.CompletedTask;
        }, cts.Token);

        // Assert
        // Verifica se o callback não foi chamado (porque não há mensagens)
        Assert.False(messageProcessed);
    }

    [Fact]
    public async Task RegisterConsumerAsync_ShouldProcessMultipleMessagesInOrder()
    {
        // Arrange
        var queueName = "testQueue";
        var message1 = new Message("First Message");
        var message2 = new Message("Second Message");
        var messageProcessedCount = 0;

        // Simula a fila retornando mensagens em ordem
        _mockQueueManager
            .SetupSequence(qm => qm.ConsumeMessageAsync(queueName))
            .ReturnsAsync(message1)
            .ReturnsAsync(message2)
            .ReturnsAsync((Message?)null); // Fila vazia após as duas mensagens

        using var cts = new CancellationTokenSource();

        // Act
        await _consumerService.RegisterConsumerAsync(queueName, async (msg) =>
        {
            if (messageProcessedCount == 0)
            {
                Assert.Equal("First Message", msg.GetBodyAsString());
            }
            else if (messageProcessedCount == 1)
            {
                Assert.Equal("Second Message", msg.GetBodyAsString());
                cts.Cancel(); // Cancela após processar a segunda mensagem
            }

            messageProcessedCount++;
            await Task.CompletedTask;
        }, cts.Token);


        // Assert
        // Verifica se o callback foi chamado duas vezes, uma para cada mensagem
        Assert.Equal(2, messageProcessedCount);
    }
}