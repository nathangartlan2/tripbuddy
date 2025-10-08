using Microsoft.Extensions.Options;
using OpenAI.Chat;
using OpenAI.Embeddings;
using TripBuddy.API.Configuration;
using TripBuddy.API.Models;

namespace TripBuddy.API.Services
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

CRITICAL RESTRICTIONS - FOLLOW EXACTLY:
- You MUST ONLY use the exact information provided in the park data below
- DO NOT mention ANY wildlife, animals, or creatures unless explicitly listed in the Features or Activities
- DO NOT add ANY details about park amenities, services, or characteristics not mentioned in the data
- DO NOT use your general knowledge about these parks - ONLY use the provided information
- If Features list says 'wildlife' generically, do NOT specify what types of wildlife
- If the user asks about something not in the park data, say 'This information is not available in our current park data'
- Use ONLY the exact text from: Name, Location, Description, ParkType, Features, Activities
- Quote similarity scores when discussing relevance

FORBIDDEN ACTIONS:
- Do not mention specific animals (bears, elk, deer, etc.) unless explicitly listed
- Do not add information about park facilities, visitor centers, or services
- Do not mention seasonal information or best times to visit
- Do not add details about park history beyond what's in Description
- Do not suggest specific trails, campsites, or locations within parks

Provide responses that are helpful but use EXCLUSIVELY the provided park data with no external knowledge added.";

                var userPrompt = $@"
User Query: {userQuery}

Retrieved Park Data (USE ONLY THIS INFORMATION):
{parkInfo}

Respond using ONLY the information above. Do not add any external knowledge about these parks.";

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