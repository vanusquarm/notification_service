using GTBStatementService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace GTBStatementService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GTB Statement Service is starting.");

            int intervalMinutes = int.Parse(_configuration["StatementSettings:ProcessIntervalMinutes"] ?? "60");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using (var scope = _serviceProvider.CreateScope())
                {
                    try
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<StatementProcessor>();
                        await processor.ProcessStatementsAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An unhandled error occurred during the statement process cycle.");
                    }
                }

                _logger.LogInformation("Worker sleeping for {Minutes} minutes...", intervalMinutes);
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
        }
    }
}
