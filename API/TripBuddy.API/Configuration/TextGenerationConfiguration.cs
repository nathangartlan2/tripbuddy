namespace TripBuddy.API.Configuration
{
    public class OpenAIConfiguration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ChatModel { get; set; } = "gpt-4o-mini";
        public string EmbeddingModel { get; set; } = "text-embedding-3-small";
    }

    public class TextGenerationConfiguration
    {
        public string Provider { get; set; } = "OpenAI"; // Only OpenAI supported
        public OpenAIConfiguration OpenAI { get; set; } = new();
    }
}