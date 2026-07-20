using Microsoft.AspNetCore.Mvc;
using PetHealthAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace PetHealthAPI.Controllers {
    /// <summary>
    /// Controller for handling executive-level operations.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] 
    public class ExecutiveController : ControllerBase {
        private readonly ICustomerIntelligenceService _intelligenceService;

        public ExecutiveController(ICustomerIntelligenceService intelligenceService) {
            _intelligenceService = intelligenceService;
        }
        /// <summary>
        /// Retrieves a consolidated summary of the enterprise operating model.
        /// </summary>
        /// <returns></returns>
        [HttpGet("operating-model-summary")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetSummary() {
            var summary = await _intelligenceService.GetEnterpriseOperatingModelAsync();
            return Ok(new { 
                IsSuccess = true, 
                Message = "Enterprise Operating Model Consolidated", 
                Data = summary 
            });
        }
    }
}