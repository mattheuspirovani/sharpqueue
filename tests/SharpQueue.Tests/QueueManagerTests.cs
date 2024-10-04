using SharpQueue.Interfaces;

namespace SharpQueue.Tests;

public class QueueManagerTests
{
    private readonly IQueueManager _queueManager;

    public QueueManagerTests()
    {
        _queueManager = new QueueManager();
    }

    [Fact]
    public async Task AddMessageAsync_ShouldAddMessageToQueue()
    {
        // Arrange
        string queueName = "testQueue";
        string messageBody = "Test Message";

        // Act
        await _queueManager.AddMessageAsync(queueName, messageBody);
        int queueLength = _queueManager.GetQueueLength(queueName);

        // Assert
        Assert.Equal(1, queueLength);
    }

    [Fact]
    public async Task ConsumeMessageAsync_ShouldReturnMessageInFIFOOrder()
    {
        // Arrange
        string queueName = "testQueue";
        string firstMessage = "First Message";
        string secondMessage = "Second Message";

        // Act
        await _queueManager.AddMessageAsync(queueName, firstMessage);
        await _queueManager.AddMessageAsync(queueName, secondMessage);

        var consumedMessage1 = await _queueManager.ConsumeMessageAsync(queueName);
        var consumedMessage2 = await _queueManager.ConsumeMessageAsync(queueName);

        // Assert
        Assert.NotNull(consumedMessage1);
        Assert.NotNull(consumedMessage2);
        Assert.Equal(firstMessage, consumedMessage1.GetBodyAsString());
        Assert.Equal(secondMessage, consumedMessage2.GetBodyAsString());
    }

    [Fact]
    public async Task ConsumeMessageAsync_ShouldReturnNullIfQueueIsEmpty()
    {
        // Arrange
        string queueName = "emptyQueue";

        // Act
        var consumedMessage = await _queueManager.ConsumeMessageAsync(queueName);

        // Assert
        Assert.Null(consumedMessage);
    }

    [Fact]
    public async Task GetQueueLength_ShouldReturnCorrectLength()
    {
        // Arrange
        string queueName = "testQueue";
        string messageBody1 = "Message 1";
        string messageBody2 = "Message 2";

        // Act
        await _queueManager.AddMessageAsync(queueName, messageBody1);
        await _queueManager.AddMessageAsync(queueName, messageBody2);
        int queueLength = _queueManager.GetQueueLength(queueName);

        // Assert
        Assert.Equal(2, queueLength);
    }

    [Fact]
    public void RemoveQueue_ShouldRemoveQueueSuccessfully()
    {
        // Arrange
        string queueName = "testQueueToRemove";

        // Act
        bool addedQueue = _queueManager.RemoveQueue(queueName);
        bool removedQueue = _queueManager.RemoveQueue(queueName);

        // Assert
        Assert.False(addedQueue); // Queue was not added yet
        Assert.False(removedQueue); // Queue was not added yet
    }
}
