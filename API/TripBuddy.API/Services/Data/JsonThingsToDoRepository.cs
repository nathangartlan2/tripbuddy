using System.Text.Json;
using TripBuddy.API.Models.Database;

namespace TripBuddy.API.Services.Data;

/// <summary>
/// JSON file implementation of IThingsToDoRepository using JSON data
/// </summary>
public class JsonThingsToDoRepository : IThingsToDoRepository
{
    private readonly List<ParkThingToDo> _thingsToDo;
    private readonly string _dataPath;

    public JsonThingsToDoRepository(IWebHostEnvironment environment)
    {
        _dataPath = Path.Combine(environment.ContentRootPath, "Data", "MockData", "park-things-to-do.json");
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
                return new List<ParkThingToDo>();

            var json = File.ReadAllText(_dataPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<List<ParkThingToDo>>(json, options) ?? new List<ParkThingToDo>();
        }
        catch
        {
            // In a real app, you'd log this error
            return new List<ParkThingToDo>();
        }
    }
}