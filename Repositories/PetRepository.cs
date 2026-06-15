using Microsoft.EntityFrameworkCore;
using PetHealthAPI.Data;
using PetHealthAPI.Models;

namespace PetHealthAPI.Repositories
{
    public class PetRepository : IPetRepository
    {
        private readonly AppDbContext _context;
        public PetRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Pet>> GetAllAsync() => await _context.Pets.ToListAsync();
        public async Task<Pet?> GetByIdAsync(int id) => await _context.Pets.FindAsync(id);
        public async Task AddAsync(Pet pet) { _context.Pets.Add(pet); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(Pet pet) { _context.Pets.Update(pet); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) 
        { 
            var pet = await _context.Pets.FindAsync(id);
            if (pet != null) { _context.Pets.Remove(pet); await _context.SaveChangesAsync(); }
        }
    }
}