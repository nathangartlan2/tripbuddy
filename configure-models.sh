#!/bin/bash

# TripBuddy API - Model Configuration Examples
# This script creates configuration files with different model setups

echo "🎯 TripBuddy API - Model Configuration Examples"
echo "=============================================="
echo ""

echo "📝 Choose your configuration method:"
echo "1) Cost-Optimized (Recommended for development)"
echo "2) Balanced (Good for most use cases)"
echo "3) High-Quality (Best responses, higher cost)"
echo "4) Custom (Specify your own models)"
echo ""

read -p "Enter choice (1-4): " CHOICE

# Create appsettings.Development.json with the chosen configuration
case $CHOICE in
    1)
        echo "💰 Setting up Cost-Optimized configuration..."
        CONFIG_TYPE="Cost-Optimized"
        CHAT_MODEL="gpt-4o-mini"
        EMBEDDING_MODEL="text-embedding-3-small"
        PROVIDER="OpenAI"
        COST_INFO="~\$0.15 per 1M tokens (chat) + \$0.02 per 1M tokens (embeddings)"
        ;;
    2)
        echo "⚖️ Setting up Balanced configuration..."
        CONFIG_TYPE="Balanced"
        CHAT_MODEL="gpt-4o-mini"
        EMBEDDING_MODEL="text-embedding-3-small"
        PROVIDER="OpenAI"
        COST_INFO="~\$0.15 per 1M tokens (chat) + \$0.02 per 1M tokens (embeddings)"
        ;;
    3)
        echo "🏆 Setting up High-Quality configuration..."
        CONFIG_TYPE="High-Quality"
        CHAT_MODEL="gpt-4o"
        EMBEDDING_MODEL="text-embedding-3-large"
        PROVIDER="OpenAI"
        COST_INFO="~\$2.50 per 1M tokens (chat) + \$0.13 per 1M tokens (embeddings)"
        ;;
    4)
        echo "🎨 Custom configuration..."
        echo ""
        echo "Available Chat Models:"
        echo "  - gpt-4o-mini (cheapest, good quality)"
        echo "  - gpt-4o (best quality, expensive)"
        echo "  - gpt-3.5-turbo (legacy, cheap)"
        echo ""
        read -p "Enter chat model: " CHAT_MODEL
        
        echo ""
        echo "Available Embedding Models:"
        echo "  - text-embedding-3-small (recommended)"
        echo "  - text-embedding-3-large (higher dimensions)"
        echo "  - text-embedding-ada-002 (legacy)"
        echo ""
        read -p "Enter embedding model: " EMBEDDING_MODEL
        
        echo ""
        echo "Provider (OpenAI or Llama):"
        read -p "Enter provider (default: OpenAI): " PROVIDER
        if [ -z "$PROVIDER" ]; then
            PROVIDER="OpenAI"
        fi
        
        CONFIG_TYPE="Custom"
        COST_INFO="Varies based on selected models"
        ;;
    *)
        echo "❌ Invalid choice. Exiting."
        exit 1
        ;;
esac

# Create appsettings.Development.json
cat > appsettings.Development.json << EOF
{
  "_comment": "This file is for local development configuration and is ignored by Git",
  "_configuration": "$CONFIG_TYPE Configuration",
  "OpenAI": {
    "ApiKey": "",
    "ChatModel": "$CHAT_MODEL",
    "EmbeddingModel": "$EMBEDDING_MODEL"
  },
  "TextGeneration": {
    "Provider": "$PROVIDER",
    "OpenAI": {
      "ApiKey": "",
      "ChatModel": "$CHAT_MODEL",
      "EmbeddingModel": "$EMBEDDING_MODEL"
    },
    "Llama": {
      "ApiUrl": "http://localhost:11434/api/generate",
      "Model": "llama2"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
EOF

echo "✅ $CONFIG_TYPE setup complete!"
echo "💰 Estimated cost: $COST_INFO"
echo "📁 Configuration saved to: appsettings.Development.json"

echo ""
echo "� Don't forget to set your API key as a secret:"
echo "   dotnet user-secrets set \"OpenAI:ApiKey\" \"your-key-here\""

echo ""
echo "📋 Current Model Configuration:"
echo "   Chat Model: $CHAT_MODEL"
echo "   Embedding Model: $EMBEDDING_MODEL"
echo "   Provider: $PROVIDER"

echo ""
echo "🚀 Ready to test! Run:"
echo "   dotnet run --urls=https://localhost:5001"
echo ""
echo "🧪 Test query example:"
echo "curl -k -X POST https://localhost:5001/api/search \\"
echo "  -H \"Content-Type: application/json\" \\"
echo "  -d '{\"query\": \"waterfalls and hiking\", \"limit\": 3}'"