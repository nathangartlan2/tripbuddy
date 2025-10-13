using TripBuddy.API.Models;
using TripBuddy.API.Models.Database;

namespace TripBuddy.API.Services.Business;

/// <summary>
/// Business service interface for park-related operations
/// </summary>
public interface IParkService
{
    Task<IEnumerable<ParkDto>> GetAllParksAsync(bool includeThingsToDo = false);
    Task<ParkDto?> GetParkByIdAsync(int id, bool includeThingsToDo = false);
    Task<ParkDto?> GetParkByNpsCodeAsync(string npsCode, bool includeThingsToDo = false);
    Task<IEnumerable<ParkDto>> GetParksByStateAsync(string stateCode);
    Task<IEnumerable<ParkDto>> SearchParksAsync(string searchTerm);
    Task<IEnumerable<ParkThingToDoDto>> GetParkActivitiesAsync(int parkId);
    Task<IEnumerable<ParkThingToDoDto>> SearchActivitiesAsync(string searchTerm, int? parkId = null);
    Task<ParkDto> CreateParkAsync(ParkDto parkDto);
    Task<ParkDto?> UpdateParkAsync(int id, ParkDto parkDto);
    Task<bool> DeleteParkAsync(int id);
}