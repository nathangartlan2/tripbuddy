using System.Text.Json;
using TripBuddy.API.Models.Database;

namespace TripBuddy.API.Services.Data;

/// <summary>
/// Wrapper class for NPS API response format
/// </summary>
internal class NpsThingsToDoResponse<T>
{
    public string? Total { get; set; }
    public string? Limit { get; set; }
    public string? Start { get; set; }
    public List<T> Data { get; set; } = new List<T>();
}

/// <summary>
/// Model for NPS things to do data format
/// </summary>
internal class NpsThingToDoData
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? LongDescription { get; set; }
    public string? Duration { get; set; }
    public List<string>? Tags { get; set; }
    public string? SeasonDescription { get; set; }
    public List<NpsThingToDoImage>? Images { get; set; }
    public string? AccessibilityInformation { get; set; }
    public List<NpsThingToDoFee>? Fees { get; set; }
    public string? Location { get; set; }
    public List<NpsRelatedParkInfo>? RelatedParks { get; set; }
}

internal class NpsThingToDoImage
{
    public string Url { get; set; } = string.Empty;
    public string Credit { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

internal class NpsThingToDoFee
{
    public string Cost { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

internal class NpsRelatedParkInfo
{
    public string ParkCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// JSON file implementation of IThingsToDoRepository using JSON data
/// </summary>
public class JsonThingsToDoRepository : IThingsToDoRepository
{
    private readonly List<ParkThingToDo> _thingsToDo;
    private readonly string _dataPath;
    private readonly ILogger<JsonThingsToDoRepository> _logger;

    public JsonThingsToDoRepository(IWebHostEnvironment environment, ILogger<JsonThingsToDoRepository> logger)
    {
        _logger = logger;
        _dataPath = Path.Combine(environment.ContentRootPath, "Data", "MockData", "parks-things-to-do.json");
        _thingsToDo = LoadThingsToDoFromJson();
    }

    public async Task<IEnumerable<ParkThingToDo>> GetAllAsync()
    {
        return await Task.FromResult(_thingsToDo.Where(t => t.IsActive));
    }

    public async Task<ParkThingToDo?> GetByIdAsync(int id)
    {
        return await Task.FromResult(_thingsToDo.FirstOrDefault(t => t.Id == id && t.IsActive));
    }

    public async Task<IEnumerable<ParkThingToDo>> GetByParkIdAsync(int parkId)
    {
        return await Task.FromResult(_thingsToDo.Where(t => t.ParkId == parkId && t.IsActive));
    }

    public async Task<IEnumerable<ParkThingToDo>> SearchAsync(string searchTerm)
    {
        var term = searchTerm.ToLowerInvariant();
        return await Task.FromResult(_thingsToDo.Where(t =>
            t.IsActive &&
            (t.Title.ToLowerInvariant().Contains(term) ||
             t.ShortDescription?.ToLowerInvariant().Contains(term) == true ||
             t.FullDescription?.ToLowerInvariant().Contains(term) == true ||
             t.LocationDescription?.ToLowerInvariant().Contains(term) == true ||
             t.ActivityTags?.Any(tag => tag.ToLowerInvariant().Contains(term)) == true)));
    }

    public async Task<IEnumerable<ParkThingToDo>> GetBySeasonAsync(string season)
    {
        return await Task.FromResult(_thingsToDo.Where(t =>
            t.IsActive &&
            t.Season?.ToLowerInvariant().Contains(season.ToLowerInvariant()) == true));
    }

    public async Task<IEnumerable<ParkThingToDo>> GetByActivityTagsAsync(string[] activityTags)
    {
        var tagsLower = activityTags.Select(tag => tag.ToLowerInvariant()).ToArray();
        return await Task.FromResult(_thingsToDo.Where(t =>
            t.IsActive &&
            t.ActivityTags?.Any(tag => tagsLower.Contains(tag.ToLowerInvariant())) == true));
    }

    public async Task<ParkThingToDo> CreateAsync(ParkThingToDo thingToDo)
    {
        thingToDo.Id = _thingsToDo.Count > 0 ? _thingsToDo.Max(t => t.Id) + 1 : 1;
        thingToDo.CreatedAt = DateTime.UtcNow;
        thingToDo.UpdatedAt = DateTime.UtcNow;
        _thingsToDo.Add(thingToDo);
        return await Task.FromResult(thingToDo);
    }

    public async Task<ParkThingToDo?> UpdateAsync(ParkThingToDo thingToDo)
    {
        var existing = _thingsToDo.FirstOrDefault(t => t.Id == thingToDo.Id);
        if (existing == null)
            return null;

        // Update properties
        existing.Title = thingToDo.Title;
        existing.ShortDescription = thingToDo.ShortDescription;
        existing.FullDescription = thingToDo.FullDescription;
        existing.Season = thingToDo.Season;
        existing.ActivityTags = thingToDo.ActivityTags;
        existing.UpdatedAt = DateTime.UtcNow;
        // ... update other properties as needed

        return await Task.FromResult(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var thingToDo = _thingsToDo.FirstOrDefault(t => t.Id == id);
        if (thingToDo == null)
            return false;

        thingToDo.IsActive = false;
        thingToDo.UpdatedAt = DateTime.UtcNow;
        return await Task.FromResult(true);
    }

    private List<ParkThingToDo> LoadThingsToDoFromJson()
    {
        try
        {
            if (!File.Exists(_dataPath))
            {
                _logger.LogWarning("Park things to do JSON file not found at path: {FilePath}", _dataPath);
                return new List<ParkThingToDo>();
            }

            _logger.LogInformation("Loading park things to do from JSON file: {FilePath}", _dataPath);

            var json = File.ReadAllText(_dataPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Deserialize as NPS API response format first
            var npsResponse = JsonSerializer.Deserialize<NpsThingsToDoResponse<NpsThingToDoData>>(json, options);
            var npsThingsToDo = npsResponse?.Data ?? new List<NpsThingToDoData>();

            // Map NPS data to internal ParkThingToDo model
            var thingsToDo = npsThingsToDo.Select((npsThingToDo, index) =>
            {
                // Extract park ID from related parks (default to 1 if not found)
                var parkId = 1; // Default fallback
                var parkCode = npsThingToDo.RelatedParks?.FirstOrDefault()?.ParkCode;
                if (!string.IsNullOrEmpty(parkCode))
                {
                    // Map park codes to IDs (this should match your parks data)
                    parkId = MapParkCodeToId(parkCode);
                }

                return new ParkThingToDo
                {
                    Id = index + 1, // Generate sequential IDs since NPS uses string GUIDs
                    ParkId = parkId,
                    NpsThingId = npsThingToDo.Id,
                    Title = npsThingToDo.Title,
                    ShortDescription = npsThingToDo.ShortDescription,
                    FullDescription = npsThingToDo.LongDescription,
                    Duration = npsThingToDo.Duration,
                    Season = npsThingToDo.SeasonDescription,
                    ActivityTags = npsThingToDo.Tags?.ToArray(),
                    AccessibilityInfo = npsThingToDo.AccessibilityInformation,
                    FeeInfo = npsThingToDo.Fees?.FirstOrDefault()?.Description,
                    LocationDescription = npsThingToDo.Location,
                    Url = npsThingToDo.Url,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }).ToList();

            _logger.LogInformation("Successfully loaded {Count} park things to do from JSON", thingsToDo.Count);

            return thingsToDo;
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Failed to deserialize park things to do JSON from {FilePath}. Invalid JSON format.", _dataPath);
            return new List<ParkThingToDo>();
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "IO error while reading park things to do JSON from {FilePath}", _dataPath);
            return new List<ParkThingToDo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading park things to do from JSON file {FilePath}", _dataPath);
            return new List<ParkThingToDo>();
        }
    }

    /// <summary>
    /// Maps NPS park codes to internal park IDs
    /// This should match the park codes from the parks JSON data
    /// </summary>
    private static int MapParkCodeToId(string parkCode)
    {
        return parkCode.ToLowerInvariant() switch
        {
            "dena" => 1,  // Denali National Park & Preserve
            "glac" => 2,  // Glacier National Park
            "grte" => 3,  // Grand Teton National Park
            "isro" => 4,  // Isle Royale National Park
            "voya" => 5,  // Voyageurs National Park
            "yell" => 6,  // Yellowstone National Park
            "yose" => 7,  // Yosemite National Park
            _ => 1        // Default to first park if not found
        };
    }
}