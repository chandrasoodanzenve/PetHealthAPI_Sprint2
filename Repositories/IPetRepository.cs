using PetHealthAPI.Models;

namespace PetHealthAPI.Repositories
{
    public interface IPetRepository
    {
        Task<(IEnumerable<Pet> Pets, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<PetSummaryDto>> GetPetSummariesAsync();
        Task<Pet?> GetByIdAsync(int id);
        Task<Pet?> GetPetByNameAndBreedAsync(string name, string breed);
        Task AddAsync(Pet pet);
        Task UpdateAsync(Pet pet);
        Task DeleteAsync(int id);
    }
}