using Microsoft.EntityFrameworkCore;
using PetHealthAPI.Models; 

namespace PetHealthAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        public DbSet<IdempotentRequest> IdempotentRequests { get; set; }
        public DbSet<PetEvent> PetEvents { get; set; }

    }
}