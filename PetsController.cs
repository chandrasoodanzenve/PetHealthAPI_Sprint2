using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHealthAPI.Data;
using PetHealthAPI.Models;

namespace PetHealthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PetsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<PetsController> _logger;

        public PetsController(AppDbContext context, IConfiguration config, ILogger<PetsController> logger)
        {
            _context = context;
             _config = config;
            _logger = logger;
        } 
        /// <summary>
        /// Provides basic information about the API and its configuration.
        /// </summary>
        [HttpGet("info")]
        public IActionResult GetInfo()
        {
   
            var appName = _config["PetSettings:AppName"] ?? "Pet Pulse API"; 
            var provider = _config["DatabaseProvider"] ?? "Not Set"; 

            return Ok(new { 
              Message = $"Welcome to {appName}", 
                 Database = provider,
                 Status = "Running"
         });
        }
        /// <summary>
        /// Retrieves a specific pet record by its ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Pet>> GetById(int id)
            {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return NotFound("Pet not found");
    
             _logger.LogInformation($"Retrieved details for Pet ID: {id}");
                return Ok(pet);
            }
        /// <summary>
        /// Retrieves all pet records from the SQL database.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pet>>> Get()
        {
            
            _logger.LogInformation("Retrieving all pets.");
            return await _context.Pets.ToListAsync();
        }
        /// <summary>
        /// Adds a new pet record to the SQL database.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Pet>> Post([FromBody] Pet pet)
        {
            _logger.LogInformation("Adding new pet.");
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();
            return Ok($"{pet.Name} added to database!");
        }
        /// <summary>
        /// Updates an existing pet record in the SQL database.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Pet updatedPet)
        {
            _logger.LogInformation("Updating pet.");
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return NotFound("Pet not found");

            pet.Name = updatedPet.Name;
            pet.Breed = updatedPet.Breed;
            pet.HealthScore = updatedPet.HealthScore;

            await _context.SaveChangesAsync();
            return Ok($"{pet.Name} updated in database!");
        }
        /// <summary>
        /// Deletes a pet record from the SQL database.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting pet.");
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return NotFound("Pet not found");

            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();
            return Ok($"{pet.Name} deleted successfully!");
        }
    }
}