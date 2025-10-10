using Microsoft.AspNetCore.Mvc;
using TripBuddy.API.Models;
using TripBuddy.API.Services;

namespace TripBuddy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GearController : ControllerBase
    {
        private readonly IGearRecommendationService _gearService;
        private readonly ILogger<GearController> _logger;

        public GearController(
            IGearRecommendationService gearService,
            ILogger<GearController> logger)
        {
            _gearService = gearService;
            _logger = logger;
        }

        /// <summary>
        /// Generate a custom gear list based on trip context
        /// </summary>
        [HttpPost("recommendations")]
        public async Task<ActionResult<GenerateGearListResponse>> GetGearRecommendations([FromBody] GenerateGearListRequest request)
        {
            try
            {
                var response = await _gearService.GenerateCustomGearListAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate gear recommendations");
                return StatusCode(500, new { error = "Failed to generate gear recommendations" });
            }
        }

        /// <summary>
        /// Get the base gear template for a trip type (for reference)
        /// </summary>
        [HttpGet("base-template/{tripType}")]
        public ActionResult<BaseGearTemplate> GetBaseTemplate(string tripType)
        {
            try
            {
                var template = Data.BaseGearData.GetTemplateByTripType(tripType);
                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get base template for trip type {TripType}", tripType);
                return StatusCode(500, new { error = "Failed to get base template" });
            }
        }

        /// <summary>
        /// Health check for gear service
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", service = "gear-recommendations", timestamp = DateTime.UtcNow });
        }
    }
}