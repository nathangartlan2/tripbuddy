namespace TripBuddy.Configuration
{
    public class OpenAIConfiguration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ChatModel { get; set; } = "gpt-4o-mini";
        public string EmbeddingModel { get; set; } = "text-embedding-3-small";
    }

    public class TextGenerationConfiguration
    {
        public string Provider { get; set; } = "OpenAI"; // "OpenAI" or "Llama"
        public OpenAIConfiguration OpenAI { get; set; } = new();
        public LlamaConfiguration Llama { get; set; } = new();
    }

    public class LlamaConfiguration
    {
        public string ApiUrl { get; set; } = "http://localhost:11434/api/generate";
        public string Model { get; set; } = "llama2";
    }
}