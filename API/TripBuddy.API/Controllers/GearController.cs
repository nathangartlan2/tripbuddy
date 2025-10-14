using Microsoft.AspNetCore.Mvc;
using TripBuddy.API.Models;
using TripBuddy.API.Services;

namespace TripBuddy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GearController : ControllerBase
    {
        private readonly IGearRecommendationServiceFactory _serviceFactory;
        private readonly ILogger<GearController> _logger;

        public GearController(
            IGearRecommendationServiceFactory serviceFactory,
            ILogger<GearController> logger)
        {
            _serviceFactory = serviceFactory;
            _logger = logger;
        }

        /// <summary>
        /// Generate a custom gear list based on trip context
        /// </summary>
        /// <param name="request">The gear list generation request</param>
        /// <param name="useRAG">Whether to use RAG (Retrieval-Augmented Generation) service for enhanced recommendations</param>
        [HttpPost("recommendations")]
        public async Task<ActionResult<GenerateGearListResponse>> GetGearRecommendations(
            [FromBody] GenerateGearListRequest request,
            [FromQuery] bool useRAG = false)
        {
            try
            {
                // Get the appropriate service from the factory
                var gearService = _serviceFactory.GetService(useRAG);

                var response = await gearService.GenerateCustomGearListAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate gear recommendations.");
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