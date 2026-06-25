using Microsoft.AspNetCore.Mvc;
using PetHealthAPI.Models;
using PetHealthAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;

namespace PetHealthAPI.Controllers
{
    ///<summary>
    /// Controller for managing pet records in the API.
    ///</summary>
    [ApiVersion("1.0")] 
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    [ApiController]
    [EnableRateLimiting("fixed")]
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
        /// <summary> Provides API metadata and current configuration details. </summary>
        /// <returns>Returns a welcome message along with the application name, database provider, and current status of the API.</returns>
        [HttpGet("info")]
        public IActionResult GetInfo()
        {
            var appName = _config["PetSettings:AppName"] ?? "Pet Pulse API";
            var provider = _config["DatabaseProvider"] ?? "Not Set";
            var envName = _config["EnvironmentName"] ?? "Development"; 

            return Ok(new { 
        Message = $"Welcome to {appName}", 
        Database = provider,
        Environment = envName, 
        Status = "Running"
    });
        }
        /// <summary> Retrieves a specific pet record by its unique ID. </summary>
        /// <param name="id">The unique identifier of the pet to retrieve.</param>
        /// <returns>Returns the pet details if found; otherwise, returns a 404 Not Found response.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Pet>>> GetById(int id)
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null) return NotFound(ApiResponse<Pet>.Failure("Pet not found"));
            return Ok(ApiResponse<Pet>.Success(pet, "Pet details retrieved successfully."));
        }
        /// <summary> Retrieves a paginated list of pets. </summary>
        /// <param name="pageNumber">The page number to retrieve (default is 1).</param>
        /// <param name="pageSize">The number of records per page (default is 10).</param>
        /// <returns>Returns a paginated list of pets along with the total count and time taken to retrieve the data.</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<Pet>>>> Get(int pageNumber = 1, int pageSize = 10)
            {
                // throw new Exception("Simulated exception for testing global error handling.");
                var watch = System.Diagnostics.Stopwatch.StartNew(); 
                    // await Task.Delay(600); 
    
                var (pets, totalCount) = await _petService.GetAllPetsAsync(pageNumber, pageSize);
    
                watch.Stop(); 
                if (watch.ElapsedMilliseconds > 500) 
                    {
                        _logger.LogCritical("ALERT: High Latency detected on GetPets! Time: {Duration}ms", watch.ElapsedMilliseconds);
                    }
                _logger.LogInformation($"Retrieving pets page {pageNumber} took {watch.ElapsedMilliseconds}ms");

                return Ok(ApiResponse<IEnumerable<Pet>>.Success(pets, $"Total Pets: {totalCount}. Displaying page {pageNumber}. Time taken: {watch.ElapsedMilliseconds}ms"));
            }
            /// <summary> Retrieves lightweight name and breed summaries of all pets. </summary>
            /// <returns>Returns a list of pet summaries containing only the name and breed of each pet.</returns>
        [HttpGet("summaries")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PetSummaryDto>>>> GetSummaries()
            {
                _logger.LogInformation("Retrieving light-weight pet summaries.");
                var summaries = await _petService.GetPetSummariesAsync();
                return Ok(ApiResponse<IEnumerable<PetSummaryDto>>.Success(summaries, "Pet summaries retrieved successfully."));
            }
        /// <summary> Adds a new pet record to the database. </summary>
        /// <param name="pet"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Pet>>> Post([FromBody] Pet pet)
        {
            await _petService.AddPetAsync(pet)
            return CreatedAtAction(nameof(GetById), new { id = pet.Id }, ApiResponse<Pet>.Success(pet, "Pet added successfully!"));
        }
        ///<summary> Updates an existing pet record in the database. </summary>
        /// <param name="id">The unique identifier of the pet to update.</param>
        /// <param name="updatedPet">The updated pet object containing new values.</param>
        /// <returns>Returns the updated pet details if successful; otherwise, returns a 404 Not Found response if the pet does not exist.</returns>
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
        /// <summary> Deletes a specific pet record from the system (Admin only). </summary>
        /// <param name="id">The unique identifier of the pet to delete.</param>
        /// <returns>Returns a success message if the pet is deleted; otherwise, returns a 404 Not Found response if the pet does not exist.</returns>
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