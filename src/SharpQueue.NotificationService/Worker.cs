namespace SharpQueue.NotificationService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly NotificationServer _notificationServer;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        _notificationServer = new NotificationServer("127.0.0.1", 5151);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationServer is starting.");

        await _notificationServer.StartAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationServer is stopping.");
        await base.StopAsync(stoppingToken);
    }
}
