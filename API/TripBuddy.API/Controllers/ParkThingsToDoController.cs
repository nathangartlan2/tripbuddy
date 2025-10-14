using Microsoft.AspNetCore.Mvc;
using TripBuddy.API.Models;
using TripBuddy.API.Services.Business;

namespace TripBuddy.API.Controllers;

/// <summary>
/// API controller for park things to do operations
/// </summary>
[ApiController]
[Route("api/parks/things-to-do")]
[Produces("application/json")]
public class ParkThingsToDoController : ControllerBase
{
    private readonly IParkThingsToDoService _thingsToDoService;
    private readonly ILogger<ParkThingsToDoController> _logger;

    public ParkThingsToDoController(
        IParkThingsToDoService thingsToDoService,
        ILogger<ParkThingsToDoController> logger)
    {
        _thingsToDoService = thingsToDoService;
        _logger = logger;
    }

    /// <summary>
    /// Get all things to do across all parks
    /// </summary>
    /// <returns>List of all things to do</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ParkThingToDoDto>>> GetAllThingsToDo()
    {
        try
        {
            var thingsToDo = await _thingsToDoService.GetAllThingsToDoAsync();
            return Ok(thingsToDo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all things to do");
            return StatusCode(500, "An error occurred while retrieving things to do");
        }
    }

    /// <summary>
    /// Get a specific thing to do by ID
    /// </summary>
    /// <param name="id">Thing to do ID</param>
    /// <param name="includePark">Include park information in response</param>
    /// <returns>Thing to do details</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ParkThingToDoDto>> GetThingToDoById(
        int id,
        [FromQuery] bool includePark = false)
    {
        try
        {
            var thingToDo = await _thingsToDoService.GetThingToDoByIdAsync(id, includePark);
            if (thingToDo == null)
            {
                return NotFound($"Thing to do with ID {id} not found");
            }

            return Ok(thingToDo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving thing to do {ThingToDoId}", id);
            return StatusCode(500, "An error occurred while retrieving the thing to do");
        }
    }

    /// <summary>
    /// Search things to do across all parks or within a specific park
    /// </summary>
    /// <param name="q">Search query</param>
    /// <param name="parkId">Optional park ID to limit search scope</param>
    /// <returns>List of matching things to do</returns>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ParkThingToDoDto>>> SearchThingsToDo(
        [FromQuery] string q,
        [FromQuery] int? parkId = null)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest("Search query cannot be empty");
        }

        try
        {
            var thingsToDo = await _thingsToDoService.SearchThingsToDoAsync(q, parkId);
            return Ok(thingsToDo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching things to do with query {Query} and parkId {ParkId}", q, parkId);
            return StatusCode(500, "An error occurred while searching things to do");
        }
    }

    /// <summary>
    /// Get things to do by season
    /// </summary>
    /// <param name="season">Season to filter by</param>
    /// <param name="parkId">Optional park ID to limit scope</param>
    /// <returns>List of seasonal things to do</returns>
    [HttpGet("by-season/{season}")]
    public async Task<ActionResult<IEnumerable<ParkThingToDoDto>>> GetThingsToDoBySeasonAsync(
        string season,
        [FromQuery] int? parkId = null)
    {
        if (string.IsNullOrWhiteSpace(season))
        {
            return BadRequest("Season cannot be empty");
        }

        try
        {
            var thingsToDo = await _thingsToDoService.GetThingsToDoBySeasonAsync(season, parkId);
            return Ok(thingsToDo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving things to do for season {Season} and parkId {ParkId}", season, parkId);
            return StatusCode(500, "An error occurred while retrieving seasonal things to do");
        }
    }

    /// <summary>
    /// Get things to do by activity tags
    /// </summary>
    /// <param name="tags">Comma-separated activity tags</param>
    /// <param name="parkId">Optional park ID to limit scope</param>
    /// <returns>List of things to do matching the activity tags</returns>
    [HttpGet("by-tags")]
    public async Task<ActionResult<IEnumerable<ParkThingToDoDto>>> GetThingsToDoByActivityTags(
        [FromQuery] string tags,
        [FromQuery] int? parkId = null)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return BadRequest("Activity tags cannot be empty");
        }

        try
        {
            var activityTags = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(tag => tag.Trim())
                                  .ToArray();

            var thingsToDo = await _thingsToDoService.GetThingsToDoByActivityTagsAsync(activityTags, parkId);
            return Ok(thingsToDo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving things to do for tags {Tags} and parkId {ParkId}", tags, parkId);
            return StatusCode(500, "An error occurred while retrieving things to do by activity tags");
        }
    }

    /// <summary>
    /// Create a new thing to do
    /// </summary>
    /// <param name="thingToDoDto">Thing to do data</param>
    /// <returns>Created thing to do</returns>
    [HttpPost]
    public async Task<ActionResult<ParkThingToDoDto>> CreateThingToDo([FromBody] ParkThingToDoDto thingToDoDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdThingToDo = await _thingsToDoService.CreateThingToDoAsync(thingToDoDto);
            return CreatedAtAction(
                nameof(GetThingToDoById),
                new { id = createdThingToDo.Id },
                createdThingToDo);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid park ID when creating thing to do");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating thing to do");
            return StatusCode(500, "An error occurred while creating the thing to do");
        }
    }

    /// <summary>
    /// Update an existing thing to do
    /// </summary>
    /// <param name="id">Thing to do ID</param>
    /// <param name="thingToDoDto">Updated thing to do data</param>
    /// <returns>Updated thing to do</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ParkThingToDoDto>> UpdateThingToDo(
        int id,
        [FromBody] ParkThingToDoDto thingToDoDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedThingToDo = await _thingsToDoService.UpdateThingToDoAsync(id, thingToDoDto);
            if (updatedThingToDo == null)
            {
                return NotFound($"Thing to do with ID {id} not found");
            }

            return Ok(updatedThingToDo);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid park ID when updating thing to do {ThingToDoId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating thing to do {ThingToDoId}", id);
            return StatusCode(500, "An error occurred while updating the thing to do");
        }
    }

    /// <summary>
    /// Delete a thing to do
    /// </summary>
    /// <param name="id">Thing to do ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteThingToDo(int id)
    {
        try
        {
            var deleted = await _thingsToDoService.DeleteThingToDoAsync(id);
            if (!deleted)
            {
                return NotFound($"Thing to do with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting thing to do {ThingToDoId}", id);
            return StatusCode(500, "An error occurred while deleting the thing to do");
        }
    }
}

/// <summary>
/// API controller for park-specific things to do operations
/// </summary>
[ApiController]
[Route("api/parks/{parkId:int}/things-to-do")]
[Produces("application/json")]
public class ParkSpecificThingsToDoController : ControllerBase
{
    private readonly IParkThingsToDoService _thingsToDoService;
    private readonly ILogger<ParkSpecificThingsToDoController> _logger;

    public ParkSpecificThingsToDoController(
        IParkThingsToDoService thingsToDoService,
        ILogger<ParkSpecificThingsToDoController> logger)
    {
        _thingsToDoService = thingsToDoService;
        _logger = logger;
    }

    /// <summary>
    /// Get all things to do for a specific park
    /// </summary>
    /// <param name="parkId">Park ID</param>
    /// <returns>List of things to do for the specified park</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ParkThingToDoDto>>> GetThingsToDoByPark(int parkId)
    {
        try
        {
            var thingsToDo = await _thingsToDoService.GetThingsToDoByParkIdAsync(parkId);
            return Ok(thingsToDo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving things to do for park {ParkId}", parkId);
            return StatusCode(500, "An error occurred while retrieving park things to do");
        }
    }
}