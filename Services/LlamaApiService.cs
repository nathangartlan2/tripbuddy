using System.Text;
using System.Text.Json;
using TripBuddy.Models;

namespace TripBuddy.Services
{
    public class LlamaApiService : ITextGenerationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LlamaApiService> _logger;

        public LlamaApiService(HttpClient httpClient, IConfiguration configuration, ILogger<LlamaApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GenerateResponseAsync(string prompt)
        {
            try
            {
                var apiUrl = _configuration["Llama:ApiUrl"] ?? "http://localhost:11434/api/generate";
                var model = _configuration["Llama:Model"] ?? "llama2";

                var requestBody = new
                {
                    model = model,
                    prompt = prompt,
                    stream = false
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending request to LLAMA API at {ApiUrl}", apiUrl);

                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var llamaResponse = JsonSerializer.Deserialize<LlamaResponse>(responseContent);

                    return llamaResponse?.Response ?? "I couldn't generate a response at the moment.";
                }
                else
                {
                    _logger.LogWarning("LLAMA API returned error status: {StatusCode}", response.StatusCode);
                    return "I'm currently unable to access the LLAMA service. Please try again later.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling LLAMA API");
                return "I encountered an error while processing your request. Please try again.";
            }
        }

        public async Task<string> GenerateResponseAsync(string userQuery, List<ParkResult> parkData)
        {
            try
            {
                var parkInfo = string.Join("\\n\\n", parkData.Select(p =>
                    $"Park: {p.Name}\\n" +
                    $"Location: {p.Location}\\n" +
                    $"Description: {p.Description}\\n" +
                    $"Activities: {string.Join(", ", p.Activities)}\\n" +
                    $"Similarity Score: {p.Similarity:F2}"));

                var prompt = $@"Based on the following park information, provide a helpful and informative response to the user's query: '{userQuery}'

Park Information:
{parkInfo}

Please provide a natural, conversational response that highlights the most relevant parks and explains why they match the user's interests. Include specific details about activities, features, and locations.";

                return await GenerateResponseAsync(prompt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating LLAMA response for query: {Query}", userQuery);
                return "I'm sorry, I encountered an error while processing your request. Please try again.";
            }
        }
    }

    public class LlamaResponse
    {
        public string Model { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public bool Done { get; set; }
        public string Context { get; set; } = string.Empty;
        public long TotalDuration { get; set; }
        public long LoadDuration { get; set; }
        public int PromptEvalCount { get; set; }
        public long PromptEvalDuration { get; set; }
        public int EvalCount { get; set; }
        public long EvalDuration { get; set; }
    }
}