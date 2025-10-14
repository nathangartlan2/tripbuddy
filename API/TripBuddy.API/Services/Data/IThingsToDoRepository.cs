using TripBuddy.API.Models.Database;

namespace TripBuddy.API.Services.Data;

/// <summary>
/// Repository interface for park things to do data access
/// </summary>
public interface IThingsToDoRepository
{
    Task<IEnumerable<ParkThingToDo>> GetAllAsync();
    Task<ParkThingToDo?> GetByIdAsync(int id);
    Task<IEnumerable<ParkThingToDo>> GetByParkIdAsync(int parkId);
    Task<IEnumerable<ParkThingToDo>> SearchAsync(string searchTerm);
    Task<IEnumerable<ParkThingToDo>> GetBySeasonAsync(string season);
    Task<IEnumerable<ParkThingToDo>> GetByActivityTagsAsync(string[] activityTags);
    Task<ParkThingToDo> CreateAsync(ParkThingToDo thingToDo);
    Task<ParkThingToDo?> UpdateAsync(ParkThingToDo thingToDo);
    Task<bool> DeleteAsync(int id);
}