# TripBuddy API

A .NET C# Web API for the Camphand nature exploration app that provides vector search capabilities against park data stored in Supabase PostgreSQL, with AI-powered responses via OpenAI.

## Features

- **Vector Search**: Semantic search against indexed park data using PostgreSQL vector extensions
- **AI Integration**: OpenAI-powered natural language responses based on search results
- **Location Filtering**: Optional geographic filtering by latitude/longitude and radius
- **RESTful API**: Clean REST endpoints with comprehensive documentation
- **Swagger UI**: Interactive API documentation available at `/swagger`

## Prerequisites

- .NET 8.0 SDK
- Supabase account with PostgreSQL database
- OpenAI API key
- Vector extension enabled in your Supabase database (pgvector)

## Setup

### 1. Database Setup

Your Supabase database should have a table structure similar to:

```sql
-- Enable vector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Parks table with vector embeddings
CREATE TABLE parks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name TEXT NOT NULL,
    description TEXT,
    location TEXT,
    latitude DECIMAL(10,8),
    longitude DECIMAL(11,8),
    park_type TEXT,
    content_embedding vector(1536), -- OpenAI embedding dimension
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Create vector search function
CREATE OR REPLACE FUNCTION search_parks(
    query_text TEXT,
    match_count INT DEFAULT 10,
    similarity_threshold FLOAT DEFAULT 0.5,
    lat FLOAT DEFAULT NULL,
    lng FLOAT DEFAULT NULL,
    radius_km FLOAT DEFAULT NULL
)
RETURNS TABLE (
    id UUID,
    name TEXT,
    description TEXT,
    location TEXT,
    park_type TEXT,
    latitude DECIMAL,
    longitude DECIMAL,
    similarity FLOAT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    WITH search_results AS (
        SELECT
            p.id,
            p.name,
            p.description,
            p.location,
            p.park_type,
            p.latitude,
            p.longitude,
            1 - (p.content_embedding <=> embedding(query_text)::vector) AS similarity_score
        FROM parks p
        WHERE
            (similarity_threshold IS NULL OR 1 - (p.content_embedding <=> embedding(query_text)::vector) >= similarity_threshold)
            AND (
                lat IS NULL OR lng IS NULL OR radius_km IS NULL OR
                ST_DWithin(
                    ST_MakePoint(p.longitude, p.latitude)::geography,
                    ST_MakePoint(lng, lat)::geography,
                    radius_km * 1000
                )
            )
        ORDER BY similarity_score DESC
        LIMIT match_count
    )
    SELECT * FROM search_results;
END;
$$;
```

### 2. Configuration

1. Copy the configuration template:

   ```bash
   cp appsettings.json appsettings.Development.json
   ```

2. Update `appsettings.Development.json` with your credentials:
   ```json
   {
     "Supabase": {
       "Url": "https://your-project.supabase.co",
       "ApiKey": "your-supabase-anon-key"
     },
     "OpenAI": {
       "ApiKey": "your-openai-api-key"
     }
   }
   ```

### 3. Run the Application

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

The API will be available at:

- HTTPS: `https://localhost:7083`
- HTTP: `http://localhost:5196`
- Swagger UI: `https://localhost:7083/swagger`

## API Endpoints

### POST /api/search

Performs semantic search against park data and returns AI-generated responses.

**Request Body:**

```json
{
  "query": "Best hiking trails near water features",
  "maxResults": 10,
  "latitude": 40.7128,
  "longitude": -74.006,
  "radiusKm": 50
}
```

**Response:**

```json
{
  "answer": "Based on your search for hiking trails near water features...",
  "parks": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "name": "Yellowstone National Park",
      "description": "America's first national park...",
      "location": "Wyoming, Montana, Idaho",
      "latitude": 44.428,
      "longitude": -110.5885,
      "parkType": "National Park",
      "similarityScore": 0.89,
      "metadata": {}
    }
  ],
  "searchQuery": "Best hiking trails near water features",
  "processingTimeMs": 1250.5
}
```

### GET /api/search/health

Health check endpoint.

## Development

### Project Structure

```
TripBuddy/
├── Controllers/
│   └── SearchController.cs    # API endpoints
├── Models/
│   └── SearchModels.cs        # Request/response models
├── Services/
│   ├── VectorSearchService.cs # Supabase vector search
│   └── OpenAIService.cs       # OpenAI integration
├── Program.cs                 # Application configuration
└── appsettings.json          # Configuration template
```

### Adding New Features

1. **New Search Filters**: Extend `SearchRequest` model and update `VectorSearchService`
2. **Different AI Models**: Modify the OpenAI client configuration in `Program.cs`
3. **Caching**: Add Redis or in-memory caching for search results
4. **Authentication**: Implement JWT or API key authentication

## Deployment

### Environment Variables

For production deployment, set these environment variables:

```bash
Supabase__Url=https://your-project.supabase.co
Supabase__ApiKey=your-supabase-service-role-key
OpenAI__ApiKey=your-openai-api-key
```

### Docker Support

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TripBuddy.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TripBuddy.dll"]
```

## Performance Considerations

- **Vector Index**: Ensure your `content_embedding` column has a proper vector index
- **Connection Pooling**: Configure appropriate connection pool settings for Supabase
- **Rate Limiting**: Implement rate limiting for OpenAI API calls
- **Caching**: Cache frequent search results to reduce API costs

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## License

This project is part of the Camphand application suite.
