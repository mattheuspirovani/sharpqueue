namespace SharpQueue.NotificationService;

public class MessageProcessor
{
    private readonly ConsumerSubscriptionManager _subscriptionManager;

    public MessageProcessor(ConsumerSubscriptionManager subscriptionManager)
    {
        _subscriptionManager = subscriptionManager;
    }

    public async Task ProcessAsync(string message, ClientConnectionHandler clientHandler)
    {
        var parsedMessage = ParseAndValidateMessage(message, clientHandler);

        if (parsedMessage == null)
        {
            return;
        }

        await ExecuteCommandAsync(parsedMessage, clientHandler);
    }

    private ParsedMessage? ParseAndValidateMessage(string message, ClientConnectionHandler clientHandler)
    {
        var parsedMessage = ParseMessage(message);

        if (parsedMessage == null || !IsValidParsedMessage(parsedMessage))
        {
            clientHandler.SendMessageAsync("RESPONSE: ERROR\nMESSAGE: Invalid message format").Wait();
            return null;
        }

        return parsedMessage;
    }

    private ParsedMessage? ParseMessage(string message)
    {
        var parsedMessage = new ParsedMessage();
        var lines = message.Split("\n");

        foreach (var line in lines)
        {
            var parts = line.Split(':', 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();

                switch (key)
                {
                    case "COMMAND":
                        parsedMessage.Command = value;
                        break;
                    case "QUEUE":
                        parsedMessage.QueueName = value;
                        break;
                    case "MESSAGE":
                        parsedMessage.MessageContent = value.Replace("\\n", "\n");
                        break;
                }
            }
        }

        return parsedMessage;
    }

    private bool IsValidParsedMessage(ParsedMessage message)
    {
        return !string.IsNullOrEmpty(message.Command) && !string.IsNullOrEmpty(message.QueueName);
    }

    private async Task ExecuteCommandAsync(ParsedMessage parsedMessage, ClientConnectionHandler clientHandler)
    {
        switch (parsedMessage.Command)
        {
            case "SUBSCRIBE":
                await HandleSubscribeAsync(parsedMessage.QueueName, clientHandler);
                break;
            case "UNSUBSCRIBE":
                await HandleUnsubscribeAsync(parsedMessage.QueueName, clientHandler);
                break;
            case "SEND":
                await HandleSendAsync(parsedMessage.QueueName, parsedMessage.MessageContent, clientHandler);
                break;
            default:
                await clientHandler.SendMessageAsync("RESPONSE: ERROR\nMESSAGE: Unknown command");
                break;
        }
    }

    private async Task HandleSubscribeAsync(string queueName, ClientConnectionHandler clientHandler)
    {
        _subscriptionManager.SubscribeClient(queueName, clientHandler);
        await clientHandler.SendMessageAsync($"RESPONSE: SUBSCRIBED\nQUEUE: {queueName}\nSTATUS: OK");
    }

    private async Task HandleUnsubscribeAsync(string queueName, ClientConnectionHandler clientHandler)
    {
        _subscriptionManager.UnsubscribeClient(queueName, clientHandler);
        await clientHandler.SendMessageAsync($"RESPONSE: UNSUBSCRIBED\nQUEUE: {queueName}\nSTATUS: OK");
    }

    private async Task HandleSendAsync(string queueName, string messageContent, ClientConnectionHandler clientHandler)
    {
        await _subscriptionManager.NotifySubscribedClients(queueName, messageContent);
        await clientHandler.SendMessageAsync($"RESPONSE: MESSAGE_RECEIVED\nQUEUE: {queueName}\nSTATUS: OK");
    }
}


public class ParsedMessage
{
    public string Command { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public string MessageContent { get; set; } = string.Empty;
}