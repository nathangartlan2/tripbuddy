using TripBuddy.API.Models;

namespace TripBuddy.API.Services
{
    public interface IGearRecommendationService
    {
        Task<GenerateGearListResponse> GenerateCustomGearListAsync(GenerateGearListRequest request);
    }
}