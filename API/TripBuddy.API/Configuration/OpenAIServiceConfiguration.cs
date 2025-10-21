using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;

namespace TripBuddy.API.Configuration
{
    public static class OpenAIServiceConfiguration
    {
        public static void AddOpenAIClients(this IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            // Get OpenAI configuration values directly
            var apiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;
            var chatModel = configuration["OpenAI:ChatModel"] ?? "gpt-4o-mini";
            var embeddingModel = configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";

            // Validate OpenAI API Key
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("⚠️  WARNING: OpenAI API Key not configured!");
                Console.WriteLine("📚 See SECRETS_GUIDE.md for setup instructions");
                Console.WriteLine("🔧 Quick setup: dotnet user-secrets set \"OpenAI:ApiKey\" \"your-key-here\"");
            }

            // Configure OpenAI Chat Client
            services.AddSingleton<ChatClient>(_ =>
            {
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("OpenAI API Key is required. Please configure it using User Secrets, Environment Variables, or appsettings.Development.json. See SECRETS_GUIDE.md for details.");
                }
                var client = new OpenAIClient(apiKey);
                return client.GetChatClient(chatModel);
            });

            // Configure OpenAI Embedding Client
            services.AddSingleton<EmbeddingClient>(_ =>
            {
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("OpenAI API Key is required. Please configure it using User Secrets, Environment Variables, or appsettings.Development.json. See SECRETS_GUIDE.md for details.");
                }
                var client = new OpenAIClient(apiKey);
                return client.GetEmbeddingClient(embeddingModel);
            });

            // Log configuration status
            logger.LogInformation("📊 OpenAI Configuration:");
            logger.LogInformation("   API Key: {Status}",
                string.IsNullOrEmpty(apiKey) ? "❌ NOT CONFIGURED" : "✅ Configured");
            logger.LogInformation("   Chat Model: {Model}", chatModel);
            logger.LogInformation("   Embedding Model: {Model}", embeddingModel);

            if (string.IsNullOrEmpty(apiKey))
            {
                logger.LogWarning("⚠️  OpenAI API Key missing! Vector search will fail.");
                logger.LogWarning("📚 See SECRETS_GUIDE.md for configuration options");
            }
        }
    }
}
