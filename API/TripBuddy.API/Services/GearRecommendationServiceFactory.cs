using TripBuddy.API.Services;

namespace TripBuddy.API.Services
{
    public class GearRecommendationServiceFactory : IGearRecommendationServiceFactory
    {
        private readonly GearRecommendationService _standardService;
        private readonly BasicRAGGearRecommendationService _ragService;
        private readonly ILogger<GearRecommendationServiceFactory> _logger;

        public GearRecommendationServiceFactory(
            GearRecommendationService standardService,
            BasicRAGGearRecommendationService ragService,
            ILogger<GearRecommendationServiceFactory> logger)
        {
            _standardService = standardService;
            _ragService = ragService;
            _logger = logger;
        }

        public IGearRecommendationService GetService(bool useRAG = false)
        {
            var serviceType = useRAG ? "BasicRAGGearRecommendationService" : "GearRecommendationService";
            _logger.LogDebug("Factory providing {ServiceType} (useRAG: {UseRAG})", serviceType, useRAG);

            return useRAG ? _ragService : _standardService;
        }
    }
}