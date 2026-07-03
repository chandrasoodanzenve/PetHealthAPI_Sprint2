using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using PetHealthAPI.Models;
using PetHealthAPI.Repositories;
using System.Diagnostics.Metrics;
using PetHealthAPI.Data; 
using Microsoft.EntityFrameworkCore;

namespace PetHealthAPI.Services
{
    public class PetService : IPetService
    {
        private readonly Counter<int> _cacheHitCounter;
        private readonly Counter<int> _cacheMissCounter;
        private readonly IPetRepository _repository;
        private readonly IDistributedCache _cache;
        private readonly AppDbContext _context;
        private IPetRepository object1;
        private IDistributedCache object2;
        private IMeterFactory object3;
        private const string CacheKeyPrefix = "Pets_Page_";
        public PetService(IPetRepository repository, IDistributedCache cache, IMeterFactory meterFactory, AppDbContext context)
        {
            _repository = repository;
            _cache = cache;
            _context = context; 
            var meter = meterFactory.Create("PetHealthAPI.Metrics");
            _cacheHitCounter = meter.CreateCounter<int>("pet_cache_hits");
            _cacheMissCounter = meter.CreateCounter<int>("pet_cache_misses");
        }

        public PetService(IPetRepository object1, IDistributedCache object2, IMeterFactory object3)
        {
            this.object1 = object1;
            this.object2 = object2;
            this.object3 = object3;
        }

        public async Task<(IEnumerable<Pet> Pets, int TotalCount)> GetAllPetsAsync(int pageNumber, int pageSize)
        {
            string key = $"{CacheKeyPrefix}{pageNumber}_{pageSize}";
            var cachedData = await _cache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _cacheHitCounter.Add(1);
                var pets = JsonSerializer.Deserialize<IEnumerable<Pet>>(cachedData);
                var resultFromDb = await _repository.GetPagedAsync(pageNumber, pageSize);
                return (pets ?? Enumerable.Empty<Pet>(), resultFromDb.TotalCount);
            }
            var result = await _repository.GetPagedAsync(pageNumber, pageSize);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = System.TimeSpan.FromMinutes(5)
            };
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
    }
}