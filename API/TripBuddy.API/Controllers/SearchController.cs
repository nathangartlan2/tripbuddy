using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TripBuddy.API.Models;
using TripBuddy.API.Services;

namespace TripBuddy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IVectorSearchService _vectorSearchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(
            IVectorSearchService vectorSearchService,
            ILogger<SearchController> logger)
        {
            _vectorSearchService = vectorSearchService;
            _logger = logger;
        }

        /// <summary>
        /// Performs a vector search against park data and generates an AI response
        /// </summary>
        /// <param name="request">The search request containing query and optional location filters</param>
        /// <returns>AI-generated response based on relevant park data</returns>
        [HttpPost]
        public async Task<ActionResult<SearchResponse>> Search([FromBody] SearchRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Processing search request for query: {Query}", request.Query);

                // Perform vector search against park data
                var response = await _vectorSearchService.SearchParksAsync(request);

                stopwatch.Stop();
                response.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;

                _logger.LogInformation("Search completed in {ElapsedMs}ms for query: {Query}",
                    stopwatch.ElapsedMilliseconds, request.Query);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error processing search request for query: {Query}", request.Query);

                return StatusCode(500, new SearchResponse
                {
                    Query = request.Query,
                    ContextualResponse = "I'm sorry, I encountered an error while processing your search. Please try again.",
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds
                });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Index a new park into the search system
        /// </summary>
        /// <param name="parkData">Park data to index</param>
        /// <returns>Success status</returns>
        [HttpPost("index")]
        public async Task<ActionResult> IndexPark([FromBody] ParkData parkData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var success = await _vectorSearchService.IndexParkAsync(parkData);

                if (success)
                {
                    return Ok(new { Message = "Park indexed successfully", ParkId = parkData.Id });
                }
                else
                {
                    return StatusCode(500, new { Message = "Failed to index park" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing park: {ParkName}", parkData.Name);
                return StatusCode(500, new { Message = "Error indexing park", Error = ex.Message });
            }
        }
    }
}