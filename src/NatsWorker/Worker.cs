using NATS.Net;

namespace NatsWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var url = _configuration["NATS_URL"] ?? "nats://localhost:4222";

        await using var client = new NatsClient(url, "NatsWorker");

        try
        {
            await client.ConnectAsync();
            _logger.LogInformation("Connected to NATS at {Url}", url);

            await Task.WhenAll(
                ReceiveMessagesAsync(client, stoppingToken),
                SendMessagesAsync(client, stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "NATS worker stopped because of an error");
            throw;
        }
    }

    private async Task ReceiveMessagesAsync(NatsClient client, CancellationToken stoppingToken)
    {
        await foreach (var message in client.SubscribeAsync<string>("demo.messages", cancellationToken: stoppingToken))
        {
            _logger.LogInformation("Received message: {Message}", message.Data);
        }
    }

    private async Task SendMessagesAsync(NatsClient client, CancellationToken stoppingToken)
    {
        var sequence = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            var message = $"Hello from NatsWorker #{++sequence}";
            await client.PublishAsync("demo.messages", message, cancellationToken: stoppingToken);
            _logger.LogInformation("Sent message: {Message}", message);

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
