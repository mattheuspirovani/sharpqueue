using SharpQueue.Interfaces;

namespace SharpQueue;

public class ConsumerService
{
    private readonly IQueueManager _queueManager;

    public ConsumerService(IQueueManager queueManager)
    {
        _queueManager = queueManager;
    }

    // Registra um consumidor para uma fila e define um callback para processar as mensagens
    public async Task RegisterConsumerAsync(string queueName, Func<Message, Task> processMessageAsync, CancellationToken cancellationToken)
    {
        // Simula consumo contínuo de mensagens enquanto houver mensagens na fila
        await Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await _queueManager.ConsumeMessageAsync(queueName);

                if (message != null)
                {
                    try
                    {
                        // Processa a mensagem usando o callback fornecido pelo consumidor
                        await processMessageAsync(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");
                        // Poderíamos adicionar lógica de retry aqui no futuro
                    }
                }
                else
                {
                    // Se não houver mensagens, pausa um pouco antes de tentar novamente
                    await Task.Delay(1000); // Aguarda 1 segundo antes de tentar consumir novamente
                }
            }
        });
    }
}
