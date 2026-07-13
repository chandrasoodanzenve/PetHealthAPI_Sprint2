using Microsoft.EntityFrameworkCore;
using PetHealthAPI.Data;
using PetHealthAPI.Models;
using System.Diagnostics;

namespace PetHealthAPI.BackgroundServices
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessor> _logger;
        private static readonly ActivitySource ActivitySource = new("PetHealthAPI.BackgroundWorker");

        public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Processor Service (Advanced Day 33) is starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                using var activity = ActivitySource.StartActivity("ProcessOutboxMessages");
                activity?.SetTag("messaging.system", "sql_outbox");
                activity?.SetTag("job.type", "Messaging");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var messages = await context.OutboxMessages
                        .Where(m => m.ProcessedOnUtc == null && m.AttemptCount < 3)
                        .OrderBy(m => m.OccurredOnUtc) 
                        .Take(10)
                        .ToListAsync(stoppingToken);

                    foreach (var message in messages)
                    {
                        if (activity?.GetTagItem($"msg.{message.Id}.processed") != null) continue;

                        var newerAlreadyProcessed = await context.OutboxMessages.AnyAsync(m => 
                            m.Type == message.Type && 
                            m.OccurredOnUtc > message.OccurredOnUtc && 
                            m.ProcessedOnUtc != null, stoppingToken);

                        if (newerAlreadyProcessed)
                        {
                            _logger.LogWarning("Ordering: Skipping stale message {Id}", message.Id);
                            message.ProcessedOnUtc = DateTime.UtcNow; 
                            continue;
                        }
                        var alreadyDone = await context.IdempotentRequests.AnyAsync(r => r.Key == $"msg-{message.Id}");
    if (alreadyDone) {
        message.ProcessedOnUtc = DateTime.UtcNow; 
        continue;
    }

                        activity?.AddEvent(new ActivityEvent("ProcessingMessage", tags: new ActivityTagsCollection { { "message.id", message.Id } }));

                        try 
                        {
                            _logger.LogInformation("Processing Message: {Id} [TraceID: {TraceId}]", 
                                message.Id, Activity.Current?.TraceId);
                        
                            await Task.Delay(100, stoppingToken); 
                            context.IdempotentRequests.Add(new IdempotentRequest {
                                Id = Guid.NewGuid(),
                                Key = $"msg-{message.Id}",
                                Result = "Processed",
                                CreatedAt = DateTime.UtcNow
                            });

                            message.ProcessedOnUtc = DateTime.UtcNow;
                            activity?.SetTag($"msg.{message.Id}.processed", true);
                        }
                        catch (Exception ex)
                        {
                            message.AttemptCount++;
                            message.Error = ex.Message;
                            _logger.LogError(ex, "Failed to process message {Id}", message.Id);
                        }
                    }

                    await context.SaveChangesAsync(stoppingToken);
                }

                await Task.Delay(10000, stoppingToken); 
            }
        }
    }
}