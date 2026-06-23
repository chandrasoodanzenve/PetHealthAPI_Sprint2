using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using PetHealthAPI.Models;
using PetHealthAPI.Repositories;
using System.Diagnostics.Metrics;

namespace PetHealthAPI.Services
{
    public class PetService : IPetService
    {

        private readonly Counter<int> _cacheHitCounter;
        private readonly Counter<int> _cacheMissCounter;
        private readonly IPetRepository _repository;
        private readonly IDistributedCache _cache;
        private const string CacheKeyPrefix = "Pets_Page_";
        public PetService(IPetRepository repository, IDistributedCache cache, IMeterFactory meterFactory)
        {
            _repository = repository;
            _cache = cache;
            var meter = meterFactory.Create("PetHealthAPI.Metrics");
            _cacheHitCounter = meter.CreateCounter<int>("pet_cache_hits");
            _cacheMissCounter = meter.CreateCounter<int>("pet_cache_misses");
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
        public async Task AddPetAsync(Pet pet) 
        { 
            await _repository.AddAsync(pet); 
            await ClearCacheAsync(); 
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