using TripBuddy.API.Models;

namespace TripBuddy.API.Services.Business;

/// <summary>
/// Business service interface for park things to do operations
/// </summary>
public interface IParkThingsToDoService
{
    Task<IEnumerable<ParkThingToDoDto>> GetAllThingsToDoAsync();
    Task<ParkThingToDoDto?> GetThingToDoByIdAsync(int id, bool includePark = false);
    Task<IEnumerable<ParkThingToDoDto>> GetThingsToDoByParkIdAsync(int parkId);
    Task<IEnumerable<ParkThingToDoDto>> SearchThingsToDoAsync(string searchTerm, int? parkId = null);
    Task<IEnumerable<ParkThingToDoDto>> GetThingsToDoBySeasonAsync(string season, int? parkId = null);
    Task<IEnumerable<ParkThingToDoDto>> GetThingsToDoByActivityTagsAsync(string[] activityTags, int? parkId = null);
    Task<ParkThingToDoDto> CreateThingToDoAsync(ParkThingToDoDto thingToDoDto);
    Task<ParkThingToDoDto?> UpdateThingToDoAsync(int id, ParkThingToDoDto thingToDoDto);
    Task<bool> DeleteThingToDoAsync(int id);
}