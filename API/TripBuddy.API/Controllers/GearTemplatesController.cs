using Microsoft.AspNetCore.Mvc;
using TripBuddy.API.Data;

namespace TripBuddy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GearTemplatesController : ControllerBase
    {
        private readonly IGearTemplateService _gearTemplateService;
        private readonly ILogger<GearTemplatesController> _logger;

        public GearTemplatesController(
            IGearTemplateService gearTemplateService,
            ILogger<GearTemplatesController> logger)
        {
            _gearTemplateService = gearTemplateService;
            _logger = logger;
        }

        /// <summary>
        /// Get all available gear templates
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllTemplates()
        {
            try
            {
                var templates = _gearTemplateService.GetAllTemplates();
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving gear templates");
                return StatusCode(500, "Error retrieving gear templates");
            }
        }

        /// <summary>
        /// Get a specific gear template by trip type
        /// </summary>
        [HttpGet("{tripType}")]
        public async Task<IActionResult> GetTemplate(string tripType)
        {
            try
            {
                var template = _gearTemplateService.GetTemplateByTripType(tripType);
                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving gear template for trip type: {TripType}", tripType);
                return StatusCode(500, $"Error retrieving gear template for trip type: {tripType}");
            }
        }
    }
}