using TripBuddy.API.Models;

namespace TripBuddy.API.Services
{
    public interface ITextGenerationService
    {
        Task<string> GenerateResponseAsync(string prompt);
        Task<string> GenerateResponseAsync(string userQuery, List<ParkResult> parkData);
    }
}