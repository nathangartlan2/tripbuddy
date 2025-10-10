using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using TripBuddy.API.Configuration;
using TripBuddy.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "TripBuddy API",
        Version = "v1",
        Description = "Vector search API for Camphand nature exploration"
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Configure Options
builder.Services.Configure<OpenAIConfiguration>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<TextGenerationConfiguration>(builder.Configuration.GetSection("TextGeneration"));

// Get configuration for validation
var openAIConfig = builder.Configuration.GetSection("OpenAI").Get<OpenAIConfiguration>() ?? new OpenAIConfiguration();
var textGenConfig = builder.Configuration.GetSection("TextGeneration").Get<TextGenerationConfiguration>() ?? new TextGenerationConfiguration();

// Validate OpenAI API Key
if (string.IsNullOrEmpty(openAIConfig.ApiKey))
{
    Console.WriteLine("⚠️  WARNING: OpenAI API Key not configured!");
    Console.WriteLine("📚 See SECRETS_GUIDE.md for setup instructions");
    Console.WriteLine("🔧 Quick setup: dotnet user-secrets set \"OpenAI:ApiKey\" \"your-key-here\"");
}

// Configure OpenAI Chat Client
builder.Services.AddSingleton<ChatClient>(provider =>
{
    if (string.IsNullOrEmpty(openAIConfig.ApiKey))
    {
        throw new InvalidOperationException("OpenAI API Key is required. Please configure it using User Secrets, Environment Variables, or appsettings.Development.json. See SECRETS_GUIDE.md for details.");
    }
    var client = new OpenAIClient(openAIConfig.ApiKey);
    return client.GetChatClient(openAIConfig.ChatModel); // Configurable model
});

// Configure OpenAI Embedding Client
builder.Services.AddSingleton<EmbeddingClient>(provider =>
{
    if (string.IsNullOrEmpty(openAIConfig.ApiKey))
    {
        throw new InvalidOperationException("OpenAI API Key is required. Please configure it using User Secrets, Environment Variables, or appsettings.Development.json. See SECRETS_GUIDE.md for details.");
    }
    var client = new OpenAIClient(openAIConfig.ApiKey);
    return client.GetEmbeddingClient(openAIConfig.EmbeddingModel); // Configurable model
});

// Register Text Generation Service based on configuration
builder.Services.AddScoped<ITextGenerationService>(provider =>
{
    var textGenConfig = provider.GetRequiredService<IOptions<TextGenerationConfiguration>>().Value;

    if (textGenConfig.Provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
    {
        return provider.GetRequiredService<OpenAIService>();
    }
    else if (textGenConfig.Provider.Equals("Llama", StringComparison.OrdinalIgnoreCase))
    {
        return provider.GetRequiredService<LlamaApiService>();
    }
    else
    {
        // Default to OpenAI
        return provider.GetRequiredService<OpenAIService>();
    }
});

// Register application services with interfaces
builder.Services.AddHttpClient<LlamaApiService>();
builder.Services.AddScoped<IVectorSearchService, VectorSearchService>();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IGearRecommendationService, GearRecommendationService>();
builder.Services.AddScoped<OpenAIService>(); // Still register concrete class for text generation factory
builder.Services.AddScoped<LlamaApiService>();

// Add logging
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Log configuration status
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 TripBuddy API Starting...");
logger.LogInformation("📊 Configuration Status:");
logger.LogInformation("   OpenAI API Key: {Status}",
    string.IsNullOrEmpty(openAIConfig.ApiKey) ? "❌ NOT CONFIGURED" : "✅ Configured");
logger.LogInformation("   OpenAI Chat Model: {Model}", openAIConfig.ChatModel);
logger.LogInformation("   OpenAI Embedding Model: {Model}", openAIConfig.EmbeddingModel);
logger.LogInformation("   Text Generation Provider: {Provider}", textGenConfig.Provider);
logger.LogInformation("   LLAMA API URL: {Url}", textGenConfig.Llama.ApiUrl);
logger.LogInformation("   LLAMA Model: {Model}", textGenConfig.Llama.Model);

if (string.IsNullOrEmpty(openAIConfig.ApiKey))
{
    logger.LogWarning("⚠️  OpenAI API Key missing! Vector search will fail.");
    logger.LogWarning("📚 See SECRETS_GUIDE.md for configuration options");
}// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TripBuddy API v1");
        c.RoutePrefix = "swagger"; // Makes Swagger UI available at /swagger
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
