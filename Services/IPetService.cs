using PetHealthAPI.Models;

namespace PetHealthAPI.Services
{
    public interface IPetService
    {
Task<(IEnumerable<Pet> Pets, int TotalCount)> GetAllPetsAsync(int pageNumber, int pageSize);
        Task<IEnumerable<PetSummaryDto>> GetPetSummariesAsync();

        Task<Pet?> GetPetByIdAsync(int id);
        Task AddPetAsync(Pet pet);
        Task UpdatePetAsync(Pet pet);
        Task DeletePetAsync(int id);
    }
}