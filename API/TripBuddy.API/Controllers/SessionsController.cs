using Microsoft.AspNetCore.Mvc;
using TripBuddy.API.Models;
using TripBuddy.API.Services;

namespace TripBuddy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly ILogger<SessionsController> _logger;

        public SessionsController(
            ISessionService sessionService,
            ILogger<SessionsController> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new trip planning session
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SessionResponse>> CreateSession([FromBody] SessionRequest request)
        {
            try
            {
                var response = await _sessionService.CreateSessionAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create session");
                return StatusCode(500, new { error = "Failed to create session" });
            }
        }

        /// <summary>
        /// Retrieves an existing session
        /// </summary>
        [HttpGet("{sessionId}")]
        public async Task<ActionResult<SessionResponse>> GetSession(string sessionId)
        {
            try
            {
                var session = await _sessionService.GetSessionAsync(sessionId);
                if (session == null)
                    return NotFound(new { error = "Session not found or expired" });

                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get session {SessionId}", sessionId);
                return StatusCode(500, new { error = "Failed to retrieve session" });
            }
        }

        /// <summary>
        /// Deletes a session
        /// </summary>
        [HttpDelete("{sessionId}")]
        public async Task<ActionResult> DeleteSession(string sessionId)
        {
            try
            {
                var deleted = await _sessionService.DeleteSessionAsync(sessionId);
                if (!deleted)
                    return NotFound(new { error = "Session not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete session {SessionId}", sessionId);
                return StatusCode(500, new { error = "Failed to delete session" });
            }
        }

        /// <summary>
        /// Updates session context and triggers LLM processing if requested
        /// </summary>
        [HttpPatch("{sessionId}/context")]
        public async Task<ActionResult<ContextUpdateResponse>> UpdateContext(
            string sessionId,
            [FromBody] ContextUpdateRequest request)
        {
            try
            {
                var response = await _sessionService.UpdateContextAsync(sessionId, request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update context for session {SessionId}", sessionId);
                return StatusCode(500, new { error = "Failed to update context" });
            }
        }

        /// <summary>
        /// Gets AI guidance for the current session state
        /// </summary>
        [HttpPost("{sessionId}/ai/guidance")]
        public async Task<ActionResult<AIGuidanceResponse>> GetAIGuidance(
            string sessionId,
            [FromBody] AIGuidanceRequest request)
        {
            try
            {
                var response = await _sessionService.GetAIGuidanceAsync(sessionId, request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get AI guidance for session {SessionId}", sessionId);
                return StatusCode(500, new { error = "Failed to get AI guidance" });
            }
        }

        /// <summary>
        /// Validates form data for a specific section
        /// </summary>
        [HttpPost("{sessionId}/ai/validate")]
        public async Task<ActionResult<ValidationResponse>> ValidateData(
            string sessionId,
            [FromBody] ValidationRequest request)
        {
            try
            {
                var response = await _sessionService.ValidateDataAsync(sessionId, request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate data for session {SessionId}", sessionId);
                return StatusCode(500, new { error = "Failed to validate data" });
            }
        }

        /// <summary>
        /// Gets AI-powered field suggestions based on current form context
        /// </summary>
        [HttpPost("{sessionId}/ai/field-suggestion")]
        public async Task<ActionResult<FieldSuggestionResponse>> GetFieldSuggestion(
            string sessionId,
            [FromBody] FieldSuggestionRequest request)
        {
            try
            {
                var response = await _sessionService.GetFieldSuggestionAsync(sessionId, request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get field suggestion for session {SessionId}", sessionId);
                return StatusCode(500, new { error = "Failed to get field suggestion" });
            }
        }

        /// <summary>
        /// Gets smart suggestions for a specific field
        /// </summary>
        [HttpGet("{sessionId}/suggestions/{fieldName}")]
        public async Task<ActionResult<SuggestionsResponse>> GetSuggestions(
            string sessionId,
            string fieldName,
            [FromQuery] string? currentValue = null)
        {
            try
            {
                var response = await _sessionService.GetSuggestionsAsync(sessionId, fieldName, currentValue);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get suggestions for session {SessionId}", sessionId);
                return StatusCode(500, new { error = "Failed to get suggestions" });
            }
        }
    }
}