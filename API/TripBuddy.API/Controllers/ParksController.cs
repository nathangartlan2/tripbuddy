using Microsoft.AspNetCore.Mvc;
using TripBuddy.API.Models;
using TripBuddy.API.Services.Business;

namespace TripBuddy.API.Controllers;

/// <summary>
/// API controller for park-related operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ParksController : ControllerBase
{
    private readonly IParkService _parkService;
    private readonly ILogger<ParksController> _logger;

    public ParksController(IParkService parkService, ILogger<ParksController> logger)
    {
        _parkService = parkService;
        _logger = logger;
    }

    /// <summary>
    /// Get all parks
    /// </summary>
    /// <param name="includeActivities">Whether to include park activities in the response</param>
    /// <returns>List of parks</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ParkDto>>> GetAllParks([FromQuery] bool includeActivities = false)
    {
        try
        {
            var parks = await _parkService.GetAllParksAsync(includeActivities);
            return Ok(parks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all parks");
            return StatusCode(500, "An error occurred while retrieving parks");
        }
    }

    /// <summary>
    /// Get a specific park by ID
    /// </summary>
    /// <param name="id">Park ID</param>
    /// <param name="includeActivities">Whether to include park activities in the response</param>
    /// <returns>Park details</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ParkDto>> GetParkById(int id, [FromQuery] bool includeActivities = false)
    {
        try
        {
            var park = await _parkService.GetParkByIdAsync(id, includeActivities);
            if (park == null)
            {
                return NotFound($"Park with ID {id} not found");
            }

            return Ok(park);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving park with ID {ParkId}", id);
            return StatusCode(500, "An error occurred while retrieving the park");
        }
    }

    /// <summary>
    /// Get a specific park by NPS code
    /// </summary>
    /// <param name="npsCode">NPS park code (e.g., 'yose', 'grca')</param>
    /// <param name="includeActivities">Whether to include park activities in the response</param>
    /// <returns>Park details</returns>
    [HttpGet("nps/{npsCode}")]
    public async Task<ActionResult<ParkDto>> GetParkByNpsCode(string npsCode, [FromQuery] bool includeActivities = false)
    {
        try
        {
            var park = await _parkService.GetParkByNpsCodeAsync(npsCode, includeActivities);
            if (park == null)
            {
                return NotFound($"Park with NPS code '{npsCode}' not found");
            }

            return Ok(park);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving park with NPS code {NpsCode}", npsCode);
            return StatusCode(500, "An error occurred while retrieving the park");
        }
    }

    /// <summary>
    /// Get parks by state
    /// </summary>
    /// <param name="stateCode">Two-letter state code (e.g., 'CA', 'NY')</param>
    /// <returns>List of parks in the specified state</returns>
    [HttpGet("state/{stateCode}")]
    public async Task<ActionResult<IEnumerable<ParkDto>>> GetParksByState(string stateCode)
    {
        try
        {
            var parks = await _parkService.GetParksByStateAsync(stateCode);
            return Ok(parks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parks for state {StateCode}", stateCode);
            return StatusCode(500, "An error occurred while retrieving parks");
        }
    }

    /// <summary>
    /// Search parks by name, description, or location
    /// </summary>
    /// <param name="q">Search query</param>
    /// <returns>List of matching parks</returns>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ParkDto>>> SearchParks([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest("Search query cannot be empty");
        }

        try
        {
            var parks = await _parkService.SearchParksAsync(q);
            return Ok(parks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching parks with query {Query}", q);
            return StatusCode(500, "An error occurred while searching parks");
        }
    }

    /// <summary>
    /// Create a new park
    /// </summary>
    /// <param name="parkDto">Park data</param>
    /// <returns>Created park</returns>
    [HttpPost]
    public async Task<ActionResult<ParkDto>> CreatePark([FromBody] ParkDto parkDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdPark = await _parkService.CreateParkAsync(parkDto);
            return CreatedAtAction(
                nameof(GetParkById),
                new { id = createdPark.Id },
                createdPark);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating park");
            return StatusCode(500, "An error occurred while creating the park");
        }
    }

    /// <summary>
    /// Update an existing park
    /// </summary>
    /// <param name="id">Park ID</param>
    /// <param name="parkDto">Updated park data</param>
    /// <returns>Updated park</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ParkDto>> UpdatePark(int id, [FromBody] ParkDto parkDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedPark = await _parkService.UpdateParkAsync(id, parkDto);
            if (updatedPark == null)
            {
                return NotFound($"Park with ID {id} not found");
            }

            return Ok(updatedPark);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating park with ID {ParkId}", id);
            return StatusCode(500, "An error occurred while updating the park");
        }
    }

    /// <summary>
    /// Delete a park (soft delete - sets IsActive to false)
    /// </summary>
    /// <param name="id">Park ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePark(int id)
    {
        try
        {
            var result = await _parkService.DeleteParkAsync(id);
            if (!result)
            {
                return NotFound($"Park with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting park with ID {ParkId}", id);
            return StatusCode(500, "An error occurred while deleting the park");
        }
    }
}