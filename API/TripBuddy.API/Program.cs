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

// Add logging early for OpenAI configuration
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Create a temporary logger for configuration
using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole());
var configLogger = loggerFactory.CreateLogger<Program>();

// Configure OpenAI clients with logging
builder.Services.AddOpenAIClients(builder.Configuration, configLogger);

builder.Services.AddScoped<ITextGenerationService>(provider =>
// Register Text Generation Service (only OpenAI supported)
{
    return provider.GetRequiredService<OpenAIService>();
});

// Register application services with interfaces
builder.Services.AddScoped<IVectorSearchService, VectorSearchService>();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();
builder.Services.AddScoped<OpenAIService>(); // Still register concrete class for text generation factory

// Register Parks services (Repository Pattern) - Dependencies for gear services
builder.Services.AddScoped<IParkRepository, JsonParkRepository>();
builder.Services.AddScoped<IThingsToDoRepository, JsonThingsToDoRepository>();
builder.Services.AddScoped<IParkService, ParkService>();
builder.Services.AddScoped<IParkThingsToDoService, ParkThingsToDoService>();

// Register Gear Template Service - Dependency for gear recommendation services
builder.Services.AddSingleton<IGearTemplateService, GearTemplateService>();

// Register concrete gear recommendation services (depend on parks services and gear template service)
builder.Services.AddScoped<GearRecommendationService>();
builder.Services.AddScoped<BasicRAGGearRecommendationService>();

// Register gear recommendation service factory and interface
builder.Services.AddScoped<IGearRecommendationService, GearRecommendationService>();
builder.Services.AddScoped<IGearRecommendationServiceFactory, GearRecommendationServiceFactory>();

var app = builder.Build();

// Log configuration status
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 TripBuddy API Starting...");

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

// Configure the HTTP request pipeline.
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
