using TripBuddy.API.Models.Database;

namespace TripBuddy.API.Services.Data;

/// <summary>
/// Repository interface for park data access
/// </summary>
public interface IParkRepository
{
    Task<IEnumerable<Park>> GetAllAsync();
    Task<Park?> GetByIdAsync(int id);
    Task<Park?> GetByNpsCodeAsync(string npsCode);
    Task<IEnumerable<Park>> GetByStateAsync(string stateCode);
    Task<IEnumerable<Park>> SearchAsync(string searchTerm);
    Task<Park> CreateAsync(Park park);
    Task<Park?> UpdateAsync(Park park);
    Task<bool> DeleteAsync(int id);
}