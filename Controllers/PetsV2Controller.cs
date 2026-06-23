using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace PetHealthAPI.Controllers
{
    ///<summary>
    /// Controller for managing pet records in the API (Version 2.0).
    /// </summary>
    [ApiVersion("2.0")] 
    [Route("api/v{version:apiVersion}/pets")] 
    [ApiController]
    [Authorize]
    public class PetsV2Controller : ControllerBase
    {
        /// <summary>
        /// Version 2.0 API endpoint for retrieving pet information.
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { 
                Message = "Welcome to Pet Health API v2 (Enhanced)",
                Data = "This version includes additional features and improvements over v1."
            });
        }
    }
}