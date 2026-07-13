using PetHealthAPI.Models;

namespace PetHealthAPI.Services
{
    public interface IPetService
    {
        Task<(IEnumerable<Pet> Pets, int TotalCount)> GetAllPetsAsync(int pageNumber, int pageSize);
        Task<IEnumerable<PetSummaryDto>> GetPetSummariesAsync();

        Task<Pet?> GetPetByIdAsync(int id);
        Task<Pet?> GetPetByNameAndBreedAsync(string name, string breed);

        Task AddPetAsync(Pet pet);
        Task UpdatePetAsync(Pet pet);
        Task DeletePetAsync(int id);
        Task ProcessPetHealthWorkflow(int petId, string newStatus);
        Task RebuildPetStateFromEvents(int petId);

    }
}