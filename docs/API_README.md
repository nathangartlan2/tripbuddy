# TripBuddy API - Vector Search for Nature Parks

A .NET 8 Web API that uses vector embeddings and cosine similarity to search through nature parks data, with AI-powered responses via LLAMA API integration.

## 🚀 Features

- **Vector Search**: Uses OpenAI embeddings and cosine similarity to find relevant parks
- **Hardcoded Parks Data**: 10 major national parks with detailed information
- **LLAMA Integration**: Uses LLAMA API for generating contextual responses
- **Cosine Similarity**: Custom implementation for accurate park matching
- **RESTful API**: Clean endpoints for search and park indexing
- **Swagger Documentation**: Auto-generated API documentation

## 📋 Prerequisites

- .NET 8 SDK
- OpenAI API Key (for embeddings)
- LLAMA API (Ollama recommended) for text generation

## 🛠️ Setup

1. **Clone and Navigate**

   ```bash
   cd tripbuddy
   ```

2. **🔐 Configure API Keys Securely**

   **Option A: User Secrets (Recommended)**

   ```bash
   # Quick setup script
   chmod +x setup-secrets.sh
   ./setup-secrets.sh

   # Or manually:
   dotnet user-secrets set "OpenAI:ApiKey" "sk-your-key-here"
   ```

   **Option B: Environment Variables**

   ```bash
   export OpenAI__ApiKey="sk-your-key-here"
   ```

   **Option C: Development File**

   ```bash
   cp appsettings.Development.template.json appsettings.Development.json
   # Edit appsettings.Development.json with your keys
   ```

   📚 **See `SECRETS_GUIDE.md` for detailed security setup**

3. **Install Ollama (for LLAMA API)**

   ```bash
   # Install Ollama
   curl -fsSL https://ollama.ai/install.sh | sh

   # Pull LLAMA2 model
   ollama pull llama2

   # Start Ollama server
   ollama serve
   ```

4. **Build and Run**
   ```bash
   dotnet build
   dotnet run --urls=https://localhost:5001
   ```

## 🧭 API Endpoints

### Health Check

```http
GET /api/search/health
```

### Search Parks

```http
POST /api/search
Content-Type: application/json

{
  "query": "I want to find parks with waterfalls and hiking trails",
  "limit": 5
}
```

**Response:**

```json
{
  "query": "I want to find parks with waterfalls and hiking trails",
  "results": [
    {
      "id": "1",
      "name": "Yosemite National Park",
      "description": "Famous for its waterfalls, deep valleys...",
      "location": "California, USA",
      "features": ["waterfalls", "granite cliffs", "sequoia trees"],
      "activities": ["hiking", "rock climbing", "camping"],
      "similarity": 0.87,
      "latitude": 37.8651,
      "longitude": -119.5383
    }
  ],
  "contextualResponse": "Based on your interest in waterfalls and hiking...",
  "totalResults": 3,
  "processingTimeMs": 1250
}
```

### Index New Park

```http
POST /api/search/index
Content-Type: application/json

{
  "id": "new-park-1",
  "name": "Amazing Nature Reserve",
  "description": "Beautiful park with diverse wildlife",
  "location": "Colorado, USA",
  "features": ["wildlife", "mountains", "forests"],
  "activities": ["hiking", "wildlife watching", "photography"]
}
```

## 🔍 How It Works

1. **Embedding Generation**: Uses OpenAI's `text-embedding-3-small` model to convert park descriptions and user queries into vector embeddings

2. **Cosine Similarity**: Calculates similarity scores between query and park embeddings using the formula:

   ```
   similarity = (A · B) / (||A|| × ||B||)
   ```

3. **LLAMA Response**: Top matching parks are sent to LLAMA API for contextual response generation

## 🏞️ Included Parks

The API comes pre-loaded with 10 major national parks:

- Yosemite National Park (California)
- Grand Canyon National Park (Arizona)
- Yellowstone National Park (Wyoming/Montana/Idaho)
- Banff National Park (Alberta, Canada)
- Great Smoky Mountains National Park (Tennessee/North Carolina)
- Zion National Park (Utah)
- Acadia National Park (Maine)
- Rocky Mountain National Park (Colorado)
- Olympic National Park (Washington)
- Glacier National Park (Montana)

## 📊 Example Queries

**Waterfalls and Hiking:**

```json
{ "query": "waterfalls and hiking trails", "limit": 3 }
```

**Rock Climbing:**

```json
{ "query": "rock climbing granite cliffs", "limit": 2 }
```

**Mountain Views:**

```json
{ "query": "mountain peaks alpine scenery", "limit": 5 }
```

**Wildlife Watching:**

```json
{ "query": "bears wolves wildlife viewing", "limit": 3 }
```

## 🧪 Testing

### Using curl:

```bash
# Health check
curl -k https://localhost:5001/api/search/health

# Search for parks
curl -k -X POST https://localhost:5001/api/search \
  -H "Content-Type: application/json" \
  -d '{"query": "hiking and waterfalls", "limit": 3}'
```

### Using the provided test script:

```bash
chmod +x test-api.sh
./test-api.sh
```

## 🔧 Architecture

```
Controllers/
├── SearchController.cs      # Main API endpoints

Services/
├── VectorSearchService.cs   # Core search logic with cosine similarity
├── OpenAIService.cs         # OpenAI embedding generation
└── LlamaApiService.cs       # LLAMA API integration

Data/
└── ParksData.cs            # Hardcoded parks dataset

Models/
└── SearchModels.cs         # Request/response models
```

## ⚡ Performance

- **Embedding Generation**: ~200-500ms per query
- **Cosine Similarity**: < 10ms for 10 parks
- **LLAMA Response**: ~1-3 seconds (depends on model)
- **Total Response Time**: ~2-4 seconds

## 🔮 Future Enhancements

- [ ] Add more parks to the dataset
- [ ] Implement location-based filtering
- [ ] Add caching for embeddings
- [ ] Support for multiple languages
- [ ] Integration with real park APIs
- [ ] Advanced filtering (season, difficulty, etc.)

## 🛡️ Security Notes

- Uses HTTPS in production
- API keys should be stored securely (Azure Key Vault, etc.)
- CORS configured for cross-origin requests
- Input validation on all endpoints

## 📈 Swagger Documentation

Access the interactive API documentation at:

```
https://localhost:5001/swagger
```

---

**Built for Camphand Nature Exploration Platform** 🏕️

This API demonstrates modern AI-powered search capabilities using vector embeddings, cosine similarity, and LLM integration for natural language understanding of user queries about nature destinations.
