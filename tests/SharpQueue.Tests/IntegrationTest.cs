namespace SharpQueue.Tests;

public class IntegrationTest
{
    private readonly QueueManager _queueManager;
    private readonly ConsumerService _consumerService;

    public IntegrationTest()
    {
        _queueManager = new QueueManager();
        _consumerService = new ConsumerService(_queueManager);
    }

    [Fact]
    public async Task IntegrationTest_ShouldProduceAndConsumeMessages()
    {
        // Arrange
        var queueName = "integrationTestQueue";
        var cancellationTokenSource = new CancellationTokenSource();
        int messagesProcessed = 0;

        await _queueManager.AddMessageAsync(queueName, "Message 1");
        await _queueManager.AddMessageAsync(queueName, "Message 2");
        await _queueManager.AddMessageAsync(queueName, "Message 3");

        // Act 
        await _consumerService.RegisterConsumerAsync(queueName, async (message) =>
        {
            Console.WriteLine($"Message consumed: {message.GetBodyAsString()}");
            messagesProcessed++;

            if (messagesProcessed == 3) // Stop consuming after 3 messages
            {
                cancellationTokenSource.Cancel();
            }

            await Task.CompletedTask;
        }, cancellationTokenSource.Token);

        // Assert - ensure that all messages were processed
        Assert.Equal(3, messagesProcessed);
    }
}