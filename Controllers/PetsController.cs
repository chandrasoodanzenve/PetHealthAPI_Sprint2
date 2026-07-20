using Microsoft.AspNetCore.Mvc;
using PetHealthAPI.Models;
using PetHealthAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore; 
using System.Text.Json; 
using PetHealthAPI.Data;
using System.Diagnostics.Metrics;
using PetHealthAPI.Middleware;

namespace PetHealthAPI.Controllers
{
    /// <summary>
    /// Controller for managing pet records in the API (Version 1.0).
    /// </summary>
    [ApiVersion("1.0")] 
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    [ApiController]
    [EnableRateLimiting("fixed")]
    public class PetsController : ControllerBase
    {
        private readonly IPetService _petService;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<PetsController> _logger;
        private readonly IOutputCacheStore _cacheStore; 
         private readonly IHttpClientFactory _httpClientFactory;


        public PetsController(
            IPetService petService, 
            AppDbContext context,
            IConfiguration config, 
            ILogger<PetsController> logger,
             IOutputCacheStore cacheStore,
             IHttpClientFactory httpClientFactory
             )
        {
            _petService = petService;
            _context = context;
            _config = config;
            _logger = logger;
            _cacheStore = cacheStore; 
            _httpClientFactory = httpClientFactory;

        }
        /// <summary>
        /// Retrieves application information including name, database provider, and environment.
        /// </summary>
        /// <returns></returns>
        [HttpGet("info")]
        [AllowAnonymous]
        public IActionResult GetInfo()
        {
            var meter = new Meter("PetHealthAPI.Metrics");
            var counter = meter.CreateCounter<int>("api_info_hits_total");
            counter.Add(1);
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
        /// <summary>
        /// Retrieves a pet by its ID with caching and performance monitoring.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [OutputCache(PolicyName = "PetCachePolicy")] 
        public async Task<ActionResult<ApiResponse<Pet>>> GetById(int id)
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null) return NotFound(ApiResponse<Pet>.Failure("Pet not found"));
            return Ok(ApiResponse<Pet>.Success(pet, "Pet details retrieved successfully."));
        }
        /// <summary>
        /// Retrieves a paginated list of pets with performance monitoring and logging.
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [OutputCache(PolicyName = "PetCachePolicy")] 
        public async Task<ActionResult<ApiResponse<IEnumerable<Pet>>>> Get(int pageNumber = 1, int pageSize = 10)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew(); 
            
            var (pets, totalCount) = await _petService.GetAllPetsAsync(pageNumber, pageSize);
    
            watch.Stop(); 
            if (watch.ElapsedMilliseconds > 500) 
            {
                _logger.LogCritical("ALERT: High Latency detected on GetPets! Time: {Duration}ms", watch.ElapsedMilliseconds);
                ExceptionMiddleware.AnomalyDetection(watch.ElapsedMilliseconds, "GET /api/v1/Pets");
            }
            
            return Ok(ApiResponse<IEnumerable<Pet>>.Success(pets, $"Total Pets: {totalCount}. Time taken: {watch.ElapsedMilliseconds}ms"));
        }
        /// <summary>
        /// Retrieves light-weight summaries of all pets with caching and performance monitoring.
        /// </summary>
        /// <returns></returns>
        [HttpGet("summaries")]
        [OutputCache(PolicyName = "PetCachePolicy")] 
        public async Task<ActionResult<ApiResponse<IEnumerable<PetSummaryDto>>>> GetSummaries()
        {
            _logger.LogInformation("Retrieving light-weight pet summaries.");
            var summaries = await _petService.GetPetSummariesAsync();
            return Ok(ApiResponse<IEnumerable<PetSummaryDto>>.Success(summaries, "Pet summaries retrieved successfully."));
        }
/// <summary>
        /// Adds a new pet to the system with idempotency support.
/// </summary>
/// <param name="pet"></param>
/// <param name="idempotencyKey"></param>
/// <returns></returns>
[HttpPost]
[DisableRateLimiting]
public async Task<IActionResult> Post([FromBody] Pet pet, [FromHeader(Name = "X-Idempotency-Key")] string? idempotencyKey)
{
    if (string.IsNullOrEmpty(idempotencyKey))
    {
        return BadRequest(ApiResponse<object>.Failure("X-Idempotency-Key header is missing."));
    }
    var existingRequest = await _context.IdempotentRequests
        .FirstOrDefaultAsync(r => r.Key == idempotencyKey);

    if (existingRequest != null)
    {
        _logger.LogInformation("Idempotency: Duplicate request detected for key {Key}", idempotencyKey);
        return Ok(JsonSerializer.Deserialize<ApiResponse<Pet>>(existingRequest.Result));
    }

    await _petService.AddPetAsync(pet);

    _logger.LogInformation("AUDIT: User {User} CREATED Pet {PetId} ({PetName}) at {Time}. CorrelationID: {CorrelationId}", 
        User.Identity?.Name ?? "Admin", 
        pet.Id,
        pet.Name,
        DateTime.UtcNow,
        HttpContext.Items["CorrelationId"] ?? "N/A");

    var response = ApiResponse<Pet>.Success(pet, "Pet added successfully!");
    var idempotentRequest = new IdempotentRequest
    {
        Id = Guid.NewGuid(),
        Key = idempotencyKey,
        Result = JsonSerializer.Serialize(response),
        CreatedAt = DateTime.UtcNow
    };

    _context.IdempotentRequests.Add(idempotentRequest);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetById), new { id = pet.Id }, response);
}
        ///<summary>
        /// Updates an existing pet.
        ///</summary>
        ///<param name="id">The ID of the pet to update.</param>
        ///<param name="updatedPet">The updated pet object.</param>
        ///<returns>The updated pet object.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Pet updatedPet)
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null) return NotFound(ApiResponse<Pet>.Failure("Pet not found"));
            
            pet.Name = updatedPet.Name;
            pet.Breed = updatedPet.Breed;
            pet.HealthScore = updatedPet.HealthScore;
            
            await _petService.UpdatePetAsync(pet);
            await _cacheStore.EvictByTagAsync("pets_tag", default);
            
            return Ok(ApiResponse<Pet>.Success(pet, "Pet updated successfully!"));
        }
        /// <summary>
        /// Deletes a pet by ID. Only accessible to Admin users.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null) return NotFound(ApiResponse<Pet>.Failure("Pet not found"));
            
            await _petService.DeletePetAsync(id);
            _logger.LogWarning("AUDIT: User {User} DELETED Pet {Id} at {Time}. CorrelationID: {CorrelationId}", 
                User.Identity?.Name ?? "Admin", 
                id, 
                DateTime.UtcNow,
                HttpContext.Items["CorrelationId"] ?? "N/A");

            await _cacheStore.EvictByTagAsync("pets_tag", default);
            
            return Ok(ApiResponse<string>.Success(null!, "Pet deleted successfully!"));
        }
        /// <summary>
        /// Admin-only endpoint to retrieve dashboard information.
        /// </summary>
        /// <returns></returns>
        [HttpGet("admin/dashboard")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetDashboard()
        {
            return Ok(new {
                Status = "Healthy",
                HealthCheckUrl = "/health",
                MetricsUrl = "/metrics",
                TracingUrl = "http://localhost:16686",
                LoggingPath = "Logs/petapi_log.txt",
                BackupStatus = "Daily at 02:00 AM"
            });
        }
         /// <summary>
        /// [Test Only] Fault Injection endpoint to validate Polly Resilience .
        /// </summary>
        [HttpGet("test-resilience")]
[AllowAnonymous]
[DisableRateLimiting]
public IActionResult TestResilience()
{
    _logger.LogWarning("TestResilience hit - Returning 500 error for Polly test.");
    return StatusCode(500, "FORCED ERROR"); 
}
        /// <summary>
        /// Verifies the resilience of the Polly policies.
        /// </summary>
        /// <returns></returns>
   [HttpGet("verify-polly")]
[AllowAnonymous]
[DisableRateLimiting]
public async Task<IActionResult> VerifyPolly()
{
    var client = _httpClientFactory.CreateClient("ResilientClient");
    
    _logger.LogInformation("Sending request via ResilientClient...");
    
    var response = await client.GetAsync("http://localhost:5082/api/v1/Pets/test-resilience");

    if (response.IsSuccessStatusCode)
        return Ok("Polly successfully handled retries and passed!");
    
    return StatusCode(500, "Polly exhausted all retries.");
}
/// <summary>
/// Retrieves the event history for a specific pet, demonstrating CQRS and Event Sourcing.
/// </summary>
/// <param name="id"></param>
/// <returns></returns>
[HttpGet("{id}/history")]
[AllowAnonymous]
public async Task<IActionResult> GetPetHistory(int id)
{
    _logger.LogInformation("CQRS: Querying event history for Pet {Id}", id);
    var events = await _context.PetEvents
        .Where(e => e.PetId == id)
        .OrderByDescending(e => e.Timestamp)
        .ToListAsync();
        
    return Ok(ApiResponse<IEnumerable<PetEvent>>.Success(events, "Historical projection retrieved."));
}

/// <summary>
/// Endpoint to trigger a Saga workflow for a specific pet.
/// </summary>
/// <param name="id"></param>
/// <param name="status"></param>
/// <returns></returns>
[HttpPost("{id}/workflow")]
[AllowAnonymous]
public async Task<IActionResult> TriggerHealthWorkflow(int id, [FromQuery] string status)
{
    _logger.LogInformation("COMMAND: Triggering Saga Workflow for Pet {Id}", id);
    
    await _petService.ProcessPetHealthWorkflow(id, status);
    
    return Ok(ApiResponse<string>.Success(null!, "Workflow processed. Check logs for Saga status."));
}

/// <summary>
/// Endpoint to replay events for a specific pet and rebuild its state.
/// </summary>
/// <param name="id"></param>
/// <returns></returns>
[HttpPost("{id}/replay")]
[AllowAnonymous]
public async Task<IActionResult> ReplayEvents(int id)
{
    await _petService.RebuildPetStateFromEvents(id);
    return Ok(ApiResponse<string>.Success(null!, "Event replay completed. State rebuilt."));
}
    }
}