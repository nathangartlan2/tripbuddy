using System.Text.Json;
using TripBuddy.API.Models.Database;

namespace TripBuddy.API.Services.Data;

/// <summary>
/// JSON file implementation of IParkRepository using JSON data
/// </summary>
public class JsonParkRepository : IParkRepository
{
    private readonly List<Park> _parks;
    private readonly string _dataPath;

    public JsonParkRepository(IWebHostEnvironment environment)
    {
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
                return new List<Park>();

            var json = File.ReadAllText(_dataPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<List<Park>>(json, options) ?? new List<Park>();
        }
        catch
        {
            // In a real app, you'd log this error
            return new List<Park>();
        }
    }
}