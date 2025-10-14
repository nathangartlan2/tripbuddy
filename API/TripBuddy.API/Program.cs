using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using TripBuddy.API.Configuration;
using TripBuddy.API.Data;
using TripBuddy.API.Services;
using TripBuddy.API.Services.Business;
using TripBuddy.API.Services.Data;

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

// Register Text Generation Service (only OpenAI supported)
builder.Services.AddScoped<ITextGenerationService>(provider =>
{
    return provider.GetRequiredService<OpenAIService>();
});

// Register application services with interfaces
builder.Services.AddScoped<IVectorSearchService, VectorSearchService>();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();
builder.Services.AddSingleton<IGearTemplateService, GearTemplateService>();
builder.Services.AddScoped<IGearRecommendationService, GearRecommendationService>();
builder.Services.AddScoped<OpenAIService>(); // Still register concrete class for text generation factory

// Register Parks services (Repository Pattern)
builder.Services.AddScoped<IParkRepository, JsonParkRepository>();
builder.Services.AddScoped<IThingsToDoRepository, JsonThingsToDoRepository>();
builder.Services.AddScoped<IParkService, ParkService>();
builder.Services.AddScoped<IParkThingsToDoService, ParkThingsToDoService>();

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

// Initialize gear templates at startup
var gearTemplateService = app.Services.GetRequiredService<IGearTemplateService>();
var availableTemplates = gearTemplateService.GetAllTemplates();
logger.LogInformation("📋 Loaded {Count} gear templates: {Templates}",
    availableTemplates.Count(),
    string.Join(", ", availableTemplates.Select(t => t.TripType)));

// Parks services registered (logging after first successful request)
logger.LogInformation("🏞️  Parks services registered successfully");
logger.LogInformation("   Repository: JSON-based data access");
logger.LogInformation("   Endpoints: /api/parks available");

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
