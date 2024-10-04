namespace SharpQueue.Tests;

public class MessageQueueTests
{
    [Fact]
    public async Task AddMessageAsync_ShouldAddMessageToQueue()
    {
        // Arrange
        var messageQueue = new MessageQueue();
        var message = new Message("Test Message");

        // Act
        await messageQueue.AddMessageAsync(message);
        var queueLength = messageQueue.GetQueueLength();

        // Assert
        Assert.Equal(1, queueLength);
    }

    [Fact]
    public async Task ConsumeMessageAsync_ShouldReturnMessageInFIFOOrder()
    {
        // Arrange
        var messageQueue = new MessageQueue();
        var firstMessage = new Message("First Message");
        var secondMessage = new Message("Second Message");

        // Act
        await messageQueue.AddMessageAsync(firstMessage);
        await messageQueue.AddMessageAsync(secondMessage);

        var consumedMessage1 = await messageQueue.ConsumeMessageAsync();
        var consumedMessage2 = await messageQueue.ConsumeMessageAsync();

        // Assert
        Assert.NotNull(consumedMessage1);
        Assert.NotNull(consumedMessage2);
        Assert.Equal("First Message", consumedMessage1.GetBodyAsString());
        Assert.Equal("Second Message", consumedMessage2.GetBodyAsString());
    }

    [Fact]
    public async Task ConsumeMessageAsync_ShouldReturnNullWhenQueueIsEmpty()
    {
        // Arrange
        var messageQueue = new MessageQueue();

        // Act
        var consumedMessage = await messageQueue.ConsumeMessageAsync();

        // Assert
        Assert.Null(consumedMessage);
    }

    [Fact]
    public async Task GetQueueLength_ShouldReturnCorrectLength()
    {
        // Arrange
        var messageQueue = new MessageQueue();
        var message1 = new Message("Message 1");
        var message2 = new Message("Message 2");

        // Act
        await messageQueue.AddMessageAsync(message1);
        await messageQueue.AddMessageAsync(message2);

        var queueLength = messageQueue.GetQueueLength();

        // Assert
        Assert.Equal(2, queueLength);
    }

    [Fact]
    public void IsEmpty_ShouldReturnTrueWhenQueueIsEmpty()
    {
        // Arrange
        var messageQueue = new MessageQueue();

        // Act
        var isEmpty = messageQueue.IsEmpty();

        // Assert
        Assert.True(isEmpty);
    }

    [Fact]
    public async Task IsEmpty_ShouldReturnFalseWhenQueueHasMessages()
    {
        // Arrange
        var messageQueue = new MessageQueue();
        var message = new Message("Test Message");

        // Act
        await messageQueue.AddMessageAsync(message);
        var isEmpty = messageQueue.IsEmpty();

        // Assert
        Assert.False(isEmpty);
    }
}
