using Microsoft.EntityFrameworkCore;
using PetHealthAPI.Data;
using PetHealthAPI.Models;

namespace PetHealthAPI.BackgroundServices
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessor> _logger;
        public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Processor Service is starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var messages = await context.OutboxMessages
                        .Where(m => m.ProcessedOnUtc == null && m.AttemptCount < 3)
                        .Take(10)
                        .ToListAsync();

                    foreach (var message in messages)
                    {
                       try 
    {
         if (message.Type == "Critical") throw new Exception("Queue Down!");

        message.ProcessedOnUtc = DateTime.UtcNow;
    }
    catch (Exception ex)
    {
        message.AttemptCount++;
        message.Error = ex.Message;
        if (message.AttemptCount >= 3)
        {
            _logger.LogCritical("MESSAGE MOVED TO DEAD-LETTER: {Id} after 3 failed attempts.", message.Id);
        }
                    }
                    await context.SaveChangesAsync(stoppingToken);
                }

                await Task.Delay(10000, stoppingToken);
            }
        }
        }
    }
}