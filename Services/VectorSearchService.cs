using TripBuddy.Data;
using TripBuddy.Models;

namespace TripBuddy.Services
{
    public interface IVectorSearchService
    {
        Task<SearchResponse> SearchParksAsync(SearchRequest request);
        Task<bool> IndexParkAsync(ParkData parkData);
    }

    public class VectorSearchService : IVectorSearchService
    {
        private readonly IOpenAIService _openAIService;
        private readonly ITextGenerationService _textGenerationService;
        private readonly ILogger<VectorSearchService> _logger;
        private readonly List<Park> _parks;

        public VectorSearchService(IOpenAIService openAIService, ITextGenerationService textGenerationService, ILogger<VectorSearchService> logger)
        {
            _openAIService = openAIService;
            _textGenerationService = textGenerationService;
            _logger = logger;
            _parks = ParksData.GetParks();

            // Pre-generate embeddings at startup - fire and forget
            _ = Task.Run(async () => await PreGenerateEmbeddingsAsync());
        }

        public async Task<SearchResponse> SearchParksAsync(SearchRequest request)
        {
            try
            {
                _logger.LogInformation($"Starting vector search for query: {request.Query}");

                // Step 1: Generate embedding for the search query
                var queryEmbedding = await _openAIService.GenerateEmbeddingAsync(request.Query);

                _logger.LogInformation("Generated embedding for search query");

                // Step 2: Perform cosine similarity search (embeddings are pre-cached at startup)
                var searchResults = PerformCosineSearch(queryEmbedding, request.Limit ?? 10);

                _logger.LogInformation($"Found {searchResults.Count} results from vector search");

                // Step 3: Generate contextual response
                var contextualResponse = await GenerateContextualResponseAsync(request.Query, searchResults);

                return new SearchResponse
                {
                    Query = request.Query,
                    Results = searchResults,
                    ContextualResponse = contextualResponse,
                    TotalResults = searchResults.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during vector search");
                throw;
            }
        }

        private async Task PreGenerateEmbeddingsAsync()
        {
            var parksNeedingEmbeddings = _parks.Where(p => p.Embedding == null).ToList();

            if (!parksNeedingEmbeddings.Any())
            {
                _logger.LogInformation("🎯 All park embeddings already cached - ready for search!");
                return;
            }

            _logger.LogInformation("🚀 Generating embeddings for {Count} new parks...", parksNeedingEmbeddings.Count);

            var tasks = parksNeedingEmbeddings.Select(async park =>
            {
                try
                {
                    var combinedText = $"{park.Name} {park.Description} {string.Join(" ", park.Features)} {string.Join(" ", park.Activities)}";
                    park.Embedding = await _openAIService.GenerateEmbeddingAsync(combinedText);
                    _logger.LogInformation($"✅ Generated new embedding for: {park.Name}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ Error generating embedding for park: {park.Name}");
                }
            });

            await Task.WhenAll(tasks);

            var totalCached = _parks.Count(p => p.Embedding != null);
            var newlyGenerated = parksNeedingEmbeddings.Count(p => p.Embedding != null);
            _logger.LogInformation("🎉 Embedding generation complete: {New} new, {Total} total parks ready for search", newlyGenerated, totalCached);
        }

        private List<ParkResult> PerformCosineSearch(double[] queryEmbedding, int limit)
        {
            var results = new List<(Park park, double similarity)>();

            foreach (var park in _parks.Where(p => p.Embedding != null))
            {
                var similarity = CalculateCosineSimilarity(queryEmbedding, park.Embedding!);
                results.Add((park, similarity));
            }

            return results
                .OrderByDescending(r => r.similarity)
                .Take(limit)
                .Select(r => new ParkResult
                {
                    Id = r.park.Id,
                    Name = r.park.Name,
                    Description = r.park.Description,
                    Location = r.park.Location,
                    Features = r.park.Features,
                    Activities = r.park.Activities,
                    Similarity = r.similarity,
                    Latitude = r.park.Latitude,
                    Longitude = r.park.Longitude
                })
                .ToList();
        }

        private static double CalculateCosineSimilarity(double[] vectorA, double[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                throw new ArgumentException("Vectors must have the same length");

            double dotProduct = 0.0;
            double magnitudeA = 0.0;
            double magnitudeB = 0.0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                magnitudeA += vectorA[i] * vectorA[i];
                magnitudeB += vectorB[i] * vectorB[i];
            }

            magnitudeA = Math.Sqrt(magnitudeA);
            magnitudeB = Math.Sqrt(magnitudeB);

            if (magnitudeA == 0.0 || magnitudeB == 0.0)
                return 0.0;

            return dotProduct / (magnitudeA * magnitudeB);
        }

        private async Task<string> GenerateContextualResponseAsync(string query, List<ParkResult> searchResults)
        {
            try
            {
                if (!searchResults.Any())
                {
                    return "I couldn't find any parks matching your criteria. Could you try rephrasing your search or being more specific?";
                }

                // Use the proper interface method that includes park data for strict prompt control
                return await _textGenerationService.GenerateResponseAsync(query, searchResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating contextual response");
                return "I found some great parks for you, but I'm having trouble generating a detailed response right now. Please check the search results for park information.";
            }
        }

        public async Task<bool> IndexParkAsync(ParkData parkData)
        {
            try
            {
                _logger.LogInformation($"Indexing park: {parkData.Name}");

                // Generate embedding for the park description and features
                var combinedText = $"{parkData.Name} {parkData.Description} {string.Join(" ", parkData.Features)} {string.Join(" ", parkData.Activities)}";
                var embedding = await _openAIService.GenerateEmbeddingAsync(combinedText);

                // Add to in-memory parks list
                var park = new Park
                {
                    Id = parkData.Id,
                    Name = parkData.Name,
                    Description = parkData.Description,
                    Location = parkData.Location,
                    Features = parkData.Features,
                    Activities = parkData.Activities,
                    Embedding = embedding
                };

                _parks.Add(park);

                _logger.LogInformation($"Successfully indexed park: {parkData.Name}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error indexing park {parkData.Name}");
                return false;
            }
        }
    }
}