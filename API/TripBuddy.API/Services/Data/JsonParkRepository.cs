using System.Text.Json;
using TripBuddy.API.Models.Database;
using TripBuddy.API.Models.NPS;

namespace TripBuddy.API.Services.Data;

/// <summary>
/// JSON file implementation of IParkRepository using JSON data
/// </summary>
public class JsonParkRepository : IParkRepository
{
    private readonly List<Park> _parks;
    private readonly string _dataPath;
    private readonly ILogger<JsonParkRepository> _logger;

    public JsonParkRepository(IWebHostEnvironment environment, ILogger<JsonParkRepository> logger)
    {
        _logger = logger;
        _dataPath = Path.Combine(environment.ContentRootPath, "Data", "MockData", "parks.json");
        _parks = LoadParksFromJson();
    }

    public async Task<IEnumerable<Park>> GetAllAsync()
    {
        return await Task.FromResult(_parks.Where(p => p.IsActive));
    }

    public async Task<Park?> GetByIdAsync(int id)
    {
        return await Task.FromResult(_parks.FirstOrDefault(p => p.Id == id && p.IsActive));
    }

    public async Task<Park?> GetByNpsCodeAsync(string npsCode)
    {
        return await Task.FromResult(_parks.FirstOrDefault(p =>
            p.NpsParkCode?.Equals(npsCode, StringComparison.OrdinalIgnoreCase) == true && p.IsActive));
    }

    public async Task<IEnumerable<Park>> GetByStateAsync(string stateCode)
    {
        return await Task.FromResult(_parks.Where(p =>
            p.StateCode?.Equals(stateCode, StringComparison.OrdinalIgnoreCase) == true && p.IsActive));
    }

    public async Task<IEnumerable<Park>> SearchAsync(string searchTerm)
    {
        var term = searchTerm.ToLowerInvariant();
        return await Task.FromResult(_parks.Where(p =>
            p.IsActive &&
            (p.Name.ToLowerInvariant().Contains(term) ||
             p.Description?.ToLowerInvariant().Contains(term) == true ||
             p.Location?.ToLowerInvariant().Contains(term) == true ||
             p.FullDescription?.ToLowerInvariant().Contains(term) == true)));
    }

    public async Task<Park> CreateAsync(Park park)
    {
        park.Id = _parks.Count > 0 ? _parks.Max(p => p.Id) + 1 : 1;
        park.CreatedAt = DateTime.UtcNow;
        park.UpdatedAt = DateTime.UtcNow;
        _parks.Add(park);
        return await Task.FromResult(park);
    }

    public async Task<Park?> UpdateAsync(Park park)
    {
        var existingPark = _parks.FirstOrDefault(p => p.Id == park.Id);
        if (existingPark == null)
            return null;

        // Update properties
        existingPark.Name = park.Name;
        existingPark.Description = park.Description;
        existingPark.FullDescription = park.FullDescription;
        existingPark.Location = park.Location;
        existingPark.UpdatedAt = DateTime.UtcNow;
        // ... update other properties as needed

        return await Task.FromResult(existingPark);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var park = _parks.FirstOrDefault(p => p.Id == id);
        if (park == null)
            return false;

        park.IsActive = false;
        park.UpdatedAt = DateTime.UtcNow;
        return await Task.FromResult(true);
    }

    private List<Park> LoadParksFromJson()
    {
        try
        {
            if (!File.Exists(_dataPath))
            {
                _logger.LogWarning("Parks JSON file not found at path: {FilePath}", _dataPath);
                return new List<Park>();
            }

            _logger.LogInformation("Loading parks from JSON file: {FilePath}", _dataPath);

            var json = File.ReadAllText(_dataPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Deserialize as NPS API response format first
            var npsResponse = JsonSerializer.Deserialize<NpsApiResponse<NpsParkData>>(json, options);
            var npsParks = npsResponse?.Data ?? new List<NpsParkData>();

            // Map NPS data to internal Park model
            var parks = npsParks.Select((npsPark, index) =>
            {
                var physicalAddress = npsPark.Addresses?.FirstOrDefault(a => a.Type == "Physical");

                return new Park
                {
                    Id = index + 1, // Generate sequential IDs since NPS uses string GUIDs
                    Name = npsPark.FullName,
                    Description = npsPark.Description,
                    NpsParkCode = npsPark.ParkCode,
                    Latitude = ParseDecimal(npsPark.Latitude),
                    Longitude = ParseDecimal(npsPark.Longitude),
                    WebsiteUrl = npsPark.Url,
                    Location = ExtractLocationFromFullName(npsPark.FullName),
                    StateCode = physicalAddress?.StateCode,
                    City = physicalAddress?.City,
                    PostalCode = physicalAddress?.PostalCode,
                    StreetAddress = physicalAddress?.Line1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }).ToList();

            _logger.LogInformation("Successfully loaded {Count} parks from JSON", parks.Count);

            return parks;
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Failed to deserialize parks JSON from {FilePath}. Invalid JSON format.", _dataPath);
            return new List<Park>();
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "IO error while reading parks JSON from {FilePath}", _dataPath);
            return new List<Park>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading parks from JSON file {FilePath}", _dataPath);
            return new List<Park>();
        }
    }

    private static decimal? ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (decimal.TryParse(value, out var result))
            return result;

        return null;
    }

    private static string? ExtractLocationFromFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return null;

        // Extract location info from park names like "Yellowstone National Park" -> "Wyoming, Montana, Idaho"
        // For now, just return the full name as location until we have more sophisticated parsing
        return fullName;
    }

    private static string? ExtractStateCodeFromFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return null;

        // Basic state extraction - this could be enhanced with a lookup table
        // For now, return null and let it be populated later
        return null;
    }
}