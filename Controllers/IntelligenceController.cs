using Microsoft.AspNetCore.Mvc;
using PetHealthAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using PetHealthAPI.Models;
namespace PetHealthAPI.Controllers
{
    /// <summary>
    /// Controller for handling intelligence-related API endpoints.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] 
    public class IntelligenceController : ControllerBase
    {
        private readonly ICustomerIntelligenceService _intelligenceService;
        private readonly ILogger<IntelligenceController> _logger;
        public IntelligenceController(ICustomerIntelligenceService intelligenceService, ILogger<IntelligenceController> logger)
        {
            _intelligenceService = intelligenceService;
            _logger = logger;
        }
        /// <summary>
        /// Endpoint to retrieve a unified customer intelligence profile for a specific user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCustomerIntelligence(int userId)
        {
            var profile = await _intelligenceService.GetUnifiedViewAsync(userId);
            return Ok(new { 
                IsSuccess = true, 
                Message = "Unified Customer Intelligence Retrieved", 
                Data = profile 
            });
        }
        ///<summary>
        /// Endpoint to retrieve longitudinal cohort analysis for a specific user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{userId}/cohort")]
        [AllowAnonymous] 
        [DisableRateLimiting]
public async Task<IActionResult> GetCohortAnalysis(int userId) {
    var result = await _intelligenceService.GetCohortEvolutionAsync(userId);
    return Ok(new { IsSuccess = true, Message = "Longitudinal Cohort Analysis Retrieved", Data = result });
}
/// <summary>
/// Endpoint to retrieve growth prioritization report for the organization.
/// </summary>
/// <returns></returns>
[HttpGet("growth-prioritization")]
[AllowAnonymous]
public async Task<IActionResult> GetGrowthPlan() {
    var result = await _intelligenceService.GetGrowthPrioritizationAsync();
    return Ok(result);
}
/// <summary>
/// Endpoint to retrieve prescriptive decision recommendations for a specific user.
/// </summary>
/// <param name="userId"></param>
/// <returns></returns>
[HttpGet("{userId}/recommendations")]
[AllowAnonymous]
public async Task<IActionResult> GetRecommendations(int userId) {
    var result = await _intelligenceService.GetDecisionRecommendationsAsync(userId);
    return Ok(result);
}
///<summary>
/// Endpoint to submit feedback on a specific recommendation action.
/// </summary>
[HttpPost("recommendations/{actionId}/feedback")]
[AllowAnonymous]
public IActionResult SubmitFeedback(string actionId, [FromQuery] string status) {
    
    _logger.LogInformation("FEEDBACK LOOP: Action {Id} marked as {Status}", actionId, status);
    
    return Ok(new { 
        IsSuccess = true, 
        Message = $"Feedback captured for action {actionId}. Feedback Loop completed." 
    });
}
    }
}