using PetHealthAPI.Models;
using PetHealthAPI.Repositories;

namespace PetHealthAPI.Services
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _repository;
        public PetService(IPetRepository repository) => _repository = repository;

        public async Task<IEnumerable<Pet>> GetAllPetsAsync() => await _repository.GetAllAsync();
        public async Task<Pet?> GetPetByIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task AddPetAsync(Pet pet) => await _repository.AddAsync(pet);
        public async Task UpdatePetAsync(Pet pet) => await _repository.UpdateAsync(pet);
        public async Task DeletePetAsync(int id) => await _repository.DeleteAsync(id);
    }
}