using PetHealthAPI.Services;
using PetHealthAPI.Models;
using System.Diagnostics.Metrics; 
using System.Diagnostics;         

namespace PetHealthAPI.BackgroundServices
{
    public class PetHealthReminderService : BackgroundService
    {
        private readonly Histogram<double> _jobDuration;
        private readonly ILogger<PetHealthReminderService> _logger;
        private readonly IServiceProvider _serviceProvider;
        public PetHealthReminderService(
            ILogger<PetHealthReminderService> logger, 
            IServiceProvider serviceProvider, 
            IMeterFactory meterFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            var meter = meterFactory.Create("PetHealthAPI.Metrics");
            _jobDuration = meter.CreateHistogram<double>("pet_health_check_duration");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Pet Health Reminder background service started!");

            while (!stoppingToken.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();

                try 
                {
                    _logger.LogInformation("Checking pet health statuses...");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var petService = scope.ServiceProvider.GetRequiredService<IPetService>();
                        var (pets, totalCount) = await petService.GetAllPetsAsync(1, 10);
                        var alertCount = 0;

                        foreach (var pet in pets)
                        {
                            if (pet.HealthScore < 80)
                            {
                                _logger.LogWarning($"Alert {pet.Name} (ID: {pet.Id}) Health score is low ({pet.HealthScore}). Attention needed!");
                                alertCount++;
                            }
                        }
                        _logger.LogInformation($"Health Check Complete: Total {totalCount} pets checked. {alertCount} pets require attention.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Something went wrong while checking pet health scores.");
                }
                finally
                {
                    sw.Stop();
                    _jobDuration.Record(sw.Elapsed.TotalSeconds);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
{
    _logger.LogWarning("Graceful shutdown initiated. Saving state and stopping background service...");
    await base.StopAsync(cancellationToken);
}
    }
}