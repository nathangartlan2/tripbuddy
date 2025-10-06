namespace TripBuddy.Services
{
    public interface ITextGenerationService
    {
        Task<string> GenerateResponseAsync(string prompt);
        Task<string> GenerateResponseAsync(string userQuery, List<TripBuddy.Models.ParkResult> parkData);
    }
}