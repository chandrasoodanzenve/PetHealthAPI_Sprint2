using Microsoft.AspNetCore.Mvc;
using PetHealthAPI.Models;
using PetHealthAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace PetHealthAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PetsController : ControllerBase
    {
        private readonly IPetService _petService;
        private readonly IConfiguration _config;
        private readonly ILogger<PetsController> _logger;

        public PetsController(IPetService petService, IConfiguration config, ILogger<PetsController> logger)
        {
            _petService = petService;
            _config = config;
            _logger = logger;
        }

        [HttpGet("info")]
        public IActionResult GetInfo()
        {
            var appName = _config["PetSettings:AppName"] ?? "Pet Pulse API";
            var provider = _config["DatabaseProvider"] ?? "Not Set";
            return Ok(new { Message = $"Welcome to {appName}", Database = provider, Status = "Running" });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Pet>>> GetById(int id)
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null) return NotFound(ApiResponse<Pet>.Failure("Pet not found"));
            return Ok(ApiResponse<Pet>.Success(pet, "Pet details retrieved successfully."));
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<Pet>>>> Get(int pageNumber = 1, int pageSize = 10)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew(); 
    
                var (pets, totalCount) = await _petService.GetAllPetsAsync(pageNumber, pageSize);
    
                watch.Stop(); 
                _logger.LogInformation($"Retrieving pets page {pageNumber} took {watch.ElapsedMilliseconds}ms");

                return Ok(ApiResponse<IEnumerable<Pet>>.Success(pets, $"Total Pets: {totalCount}. Displaying page {pageNumber}. Time taken: {watch.ElapsedMilliseconds}ms"));
            }
        [HttpGet("summaries")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PetSummaryDto>>>> GetSummaries()
            {
                _logger.LogInformation("Retrieving light-weight pet summaries.");
                var summaries = await _petService.GetPetSummariesAsync();
                return Ok(ApiResponse<IEnumerable<PetSummaryDto>>.Success(summaries, "Pet summaries retrieved successfully."));
            }
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Pet>>> Post([FromBody] Pet pet)
        {
            await _petService.AddPetAsync(pet);
            return CreatedAtAction(nameof(GetById), new { id = pet.Id }, ApiResponse<Pet>.Success(pet, "Pet added successfully!"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Pet updatedPet)
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null) return NotFound(ApiResponse<Pet>.Failure("Pet not found"));
            pet.Name = updatedPet.Name;
            pet.Breed = updatedPet.Breed;
            pet.HealthScore = updatedPet.HealthScore;
            await _petService.UpdatePetAsync(pet);
            return Ok(ApiResponse<Pet>.Success(pet, "Pet updated successfully!"));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null) return NotFound(ApiResponse<Pet>.Failure("Pet not found"));
            await _petService.DeletePetAsync(id);
            return Ok(ApiResponse<string>.Success(null!, "Pet deleted successfully!"));
        }
    }
}