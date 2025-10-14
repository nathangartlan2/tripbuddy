using TripBuddy.API.Services;

namespace TripBuddy.API.Services
{
    public interface IGearRecommendationServiceFactory
    {
        IGearRecommendationService GetService(bool useRAG = false);
    }
}