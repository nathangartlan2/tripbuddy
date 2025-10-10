using Microsoft.AspNetCore.Mvc;
using TripBuddy.API.Models;
using TripBuddy.API.Services;

namespace TripBuddy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TemplatesController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly ILogger<TemplatesController> _logger;

        public TemplatesController(
            ISessionService sessionService,
            ILogger<TemplatesController> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a template by ID
        /// </summary>
        [HttpGet("{templateId}")]
        public async Task<ActionResult<Template>> GetTemplate(string templateId)
        {
            try
            {
                var template = await _sessionService.GetTemplateAsync(templateId);
                return Ok(template);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get template {TemplateId}", templateId);
                return StatusCode(500, new { error = "Failed to retrieve template" });
            }
        }

        /// <summary>
        /// Lists available templates
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ListTemplates()
        {
            try
            {
                var templates = new[]
                {
                    new { id = "backpacking-template-v1", name = "Backpacking Trip Planner", tripType = "backpacking" }
                };

                return Ok(new { templates });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list templates");
                return StatusCode(500, new { error = "Failed to list templates" });
            }
        }
    }
}