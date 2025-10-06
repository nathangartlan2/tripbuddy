using Microsoft.Extensions.Options;
using OpenAI.Chat;
using OpenAI.Embeddings;
using TripBuddy.Configuration;
using TripBuddy.Models;

namespace TripBuddy.Services
{
    public interface IOpenAIService
    {
        Task<double[]> GenerateEmbeddingAsync(string text);
    }

    public class OpenAIService : IOpenAIService, ITextGenerationService
    {
        private readonly ChatClient _chatClient;
        private readonly EmbeddingClient _embeddingClient;
        private readonly OpenAIConfiguration _config;
        private readonly ILogger<OpenAIService> _logger;

        public OpenAIService(ChatClient chatClient, EmbeddingClient embeddingClient, IOptions<OpenAIConfiguration> config, ILogger<OpenAIService> logger)
        {
            _chatClient = chatClient;
            _embeddingClient = embeddingClient;
            _config = config.Value;
            _logger = logger;
        }

        public async Task<double[]> GenerateEmbeddingAsync(string text)
        {
            try
            {
                var response = await _embeddingClient.GenerateEmbeddingAsync(text);
                // Convert ReadOnlyMemory<float> to double array
                var floats = response.Value.ToFloats();
                return floats.ToArray().Select(f => (double)f).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating embedding for text: {Text}", text);
                throw;
            }
        }

        public async Task<string> GenerateResponseAsync(string prompt)
        {
            try
            {
                var messages = new List<ChatMessage>
                {
                    new UserChatMessage(prompt)
                };

                var response = await _chatClient.CompleteChatAsync(messages);

                return response.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OpenAI response for prompt");
                return "I'm sorry, I encountered an error while processing your request. Please try again.";
            }
        }

        public async Task<string> GenerateResponseAsync(string userQuery, List<ParkResult> parkData)
        {
            try
            {
                var parkInfo = string.Join("\n\n", parkData.Select(p =>
                    $"Park: {p.Name}\n" +
                    $"Location: {p.Location}\n" +
                    $"Type: {p.ParkType}\n" +
                    $"Description: {p.Description}\n" +
                    $"Similarity Score: {p.Similarity:F2}"));

                var systemPrompt = @"
You are a helpful assistant for TripBuddy, a nature exploration app called Camphand. 
You help users discover and learn about parks and nature areas.

Based on the user's query and the retrieved park data, provide a helpful, informative response that:
1. Directly answers their question
2. Highlights the most relevant parks from the search results
3. Provides practical information for trip planning
4. Maintains an enthusiastic but professional tone
5. Includes specific details from the park data

Always base your response on the provided park data and avoid making up information.";

                var userPrompt = $@"
User Query: {userQuery}

Retrieved Park Data:
{parkInfo}

Please provide a helpful response based on this information.";

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userPrompt)
                };

                var response = await _chatClient.CompleteChatAsync(messages);

                return response.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OpenAI response for query: {Query}", userQuery);
                return "I'm sorry, I encountered an error while processing your request. Please try again.";
            }
        }
    }
}