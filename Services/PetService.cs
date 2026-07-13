using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using PetHealthAPI.Models;
using PetHealthAPI.Repositories;
using System.Diagnostics.Metrics;
using PetHealthAPI.Data; 
using Microsoft.EntityFrameworkCore;
using System.Diagnostics; 
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace PetHealthAPI.Services
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _repository;
        private readonly IDistributedCache _cache;
        private readonly AppDbContext _context;
        private readonly Counter<int> _cacheHitCounter;
        private readonly Counter<int> _cacheMissCounter;
        private readonly Counter<int> _petsCreatedCounter; 
        private readonly Histogram<double> _registrationDuration;
        private const string CacheKeyPrefix = "Pets_Page_";
        private readonly ILogger<PetService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public PetService(IPetRepository repository, IDistributedCache cache, IMeterFactory meterFactory, AppDbContext context, ILogger<PetService> logger, IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _cache = cache;
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            var meter = meterFactory.Create("PetHealthAPI.Metrics");
            _cacheHitCounter = meter.CreateCounter<int>("pet_cache_hits", "Hits");
            _cacheMissCounter = meter.CreateCounter<int>("pet_cache_misses", "Misses");
            _petsCreatedCounter = meter.CreateCounter<int>("pet_registrations_total", "Pets", "Total number of pets registered");
            _registrationDuration = meter.CreateHistogram<double>("pet_registration_duration", "ms", "Latency of pet registration process");
        }

        public async Task<(IEnumerable<Pet> Pets, int TotalCount)> GetAllPetsAsync(int pageNumber, int pageSize)
        {
            string key = $"{CacheKeyPrefix}{pageNumber}_{pageSize}";
            var cachedData = await _cache.GetStringAsync(key);
            
            if (!string.IsNullOrEmpty(cachedData))
            {
                _cacheHitCounter.Add(1);
                var pets = JsonSerializer.Deserialize<IEnumerable<Pet>>(cachedData);
                var totalCount = await _context.Pets.CountAsync();
                return (pets ?? Enumerable.Empty<Pet>(), totalCount);
            }

            var result = await _repository.GetPagedAsync(pageNumber, pageSize);
            
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = System.TimeSpan.FromMinutes(5)
            } ;

            await _cache.SetStringAsync(key, JsonSerializer.Serialize(result.Pets), options);
            _cacheMissCounter.Add(1);
            
            return result;
        }

        public async Task<Pet?> GetPetByIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task<Pet?> GetPetByNameAndBreedAsync(string name, string breed)
        {
            return await _repository.GetPetByNameAndBreedAsync(name, breed);
        }
        public async Task AddPetAsync(Pet pet)
        {
            var watch = Stopwatch.StartNew(); 
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _repository.AddAsync(pet);
                var message = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = "PetCreated",
                    Content = JsonSerializer.Serialize(pet),
                    OccurredOnUtc = DateTime.UtcNow,
                    AttemptCount = 0 
                };

                _context.OutboxMessages.Add(message);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                await ClearCacheAsync();

                // ---  Record Performance & Business Metrics ---
                watch.Stop();
                _petsCreatedCounter.Add(1); 
                _registrationDuration.Record(watch.ElapsedMilliseconds); 
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdatePetAsync(Pet pet) 
        { 
            await _repository.UpdateAsync(pet); 
            await ClearCacheAsync(); 
        }
        public async Task DeletePetAsync(int id) 
        { 
            await _repository.DeleteAsync(id); 
            await ClearCacheAsync(); 
        }
        public async Task<IEnumerable<PetSummaryDto>> GetPetSummariesAsync()
        {
            return await _repository.GetPetSummariesAsync();
        }

        private async Task ClearCacheAsync() 
        {
            await _cache.RemoveAsync($"{CacheKeyPrefix}1_10");
        }
        
public async Task ProcessPetHealthWorkflow(int petId, string newStatus)
{
    var resilientClient = _httpClientFactory.CreateClient("ResilientClient");

    _logger.LogWarning("SAGA: Starting resilient workflow for Pet {Id}", petId);
    // Step 1: Start Transaction 
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // 1. Update Pet Status
        var pet = await _repository.GetByIdAsync(petId);
        if (pet == null) throw new Exception("Pet not found");
        pet.Breed = "Updated via Saga"; 

        // 2. Record Event (Event Sourcing)
        var petEvent = new PetEvent {
            Id = Guid.NewGuid(),
            PetId = petId,
            EventType = "HealthWorkflowStarted",
            Data = newStatus,
            Timestamp = DateTime.UtcNow,
            Version = "v2"
        };
        _context.PetEvents.Add(petEvent);
        await _context.SaveChangesAsync();

        // 3. Simulate external service failure 
        if (newStatus == "FAIL_TEST") throw new Exception("External Workflow Failed!");

        await transaction.CommitAsync();
        _logger.LogWarning("!!! SAGA COMPLETED !!! Pet {Id} is success.", petId);

    }
    catch (Exception ex)
    {
        // COMPENSATING TRANSACTION: Rollback logic
        await transaction.RollbackAsync();
        _logger.LogError("Saga Failed! Rollback triggered for Pet {Id}. Error: {Msg}", petId, ex.Message);
        
        // Task: recovery mechanisms
        await HandleSagaRecovery(petId, ex.Message);
    }
}

private async Task HandleSagaRecovery(int petId, string reason)
{
     _logger.LogWarning("RECOVERY: Executing compensation for Pet {Id} due to: {Reason}", petId, reason);
            await Task.Delay(1);  
}
public async Task RebuildPetStateFromEvents(int petId)
{
    _logger.LogInformation("REPLAY: Rebuilding state for Pet {Id} from event store...", petId);

    // 1. Get all historical events for this pet
    var events = await _context.PetEvents
        .Where(e => e.PetId == petId)
        .OrderBy(e => e.Timestamp)
        .ToListAsync();

    // 2. Replay logic 
    foreach (var petEvent in events)
    {
        if (petEvent.Version == "v1") {
            _logger.LogInformation("Processing Legacy v1 Event: {Type}", petEvent.EventType);
            // Handle v1 logic
        }
        else if (petEvent.Version == "v2") {
            _logger.LogInformation("Processing Modern v2 Event: {Type}", petEvent.EventType);
        }
    }

    _logger.LogInformation("REPLAY COMPLETED. Projection rebuilt for Pet {Id}", petId);
}
    }
}