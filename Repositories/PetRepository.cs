using Microsoft.EntityFrameworkCore;
using PetHealthAPI.Data;
using PetHealthAPI.Models;

namespace PetHealthAPI.Repositories
{
    public class PetRepository : IPetRepository
    {
        private readonly AppDbContext _context;
        public PetRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<(IEnumerable<Pet> Pets, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Pets.AsNoTracking();
            var totalCount = await _context.Pets.CountAsync();
            var pets = await _context.Pets
                .AsNoTracking() 
                .TagWith("GetPagedPets_Performance")
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (pets, totalCount);
        }
       public async Task<Pet?> GetByIdAsync(int id)
{
        return await _context.Pets
        .AsNoTracking()
        .AsSplitQuery() 
        .FirstOrDefaultAsync(p => p.Id == id);
}
public async Task<Pet?> GetPetByNameAndBreedAsync(string name, string breed)
{
    return await _context.Pets
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Name == name && p.Breed == breed);
}

        public async Task AddAsync(Pet pet) 
        { 
            _context.Pets.Add(pet); 
            await _context.SaveChangesAsync(); 
        }
        public async Task UpdateAsync(Pet pet) 
        { 
            _context.Pets.Update(pet); 
            await _context.SaveChangesAsync(); 
        }
        public async Task DeleteAsync(int id) 
        { 
            var pet = await _context.Pets.FindAsync(id);
            if (pet != null) 
            { 
                _context.Pets.Remove(pet); 
                await _context.SaveChangesAsync(); 
            }
        }
        public async Task<IEnumerable<PetSummaryDto>> GetPetSummariesAsync()
        {
            return await _context.Pets
                .AsNoTracking()
                .Select(p => new PetSummaryDto 
                { 
                    Name = p.Name, 
                    Breed = p.Breed 
                })
                .ToListAsync();
        }
    }
}