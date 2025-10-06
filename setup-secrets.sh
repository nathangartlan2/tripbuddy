#!/bin/bash

# TripBuddy API - Quick Secrets Setup Script
# Run this script to configure your API keys securely

echo "🔐 TripBuddy API - Secrets Configuration"
echo "========================================"
echo ""

# Check if we're in the right directory
if [ ! -f "TripBuddy.csproj" ]; then
    echo "❌ Error: Please run this script from the tripbuddy project directory"
    exit 1
fi

echo "📝 This script will help you configure your API keys securely using .NET User Secrets"
echo "🔒 Your keys will NOT be committed to Git"
echo ""

# OpenAI API Key
echo "🤖 OpenAI Configuration"
echo "━━━━━━━━━━━━━━━━━━━━━━━"
echo "Please enter your OpenAI API Key (starts with 'sk-'):"
read -s OPENAI_KEY

if [ -z "$OPENAI_KEY" ]; then
    echo "⚠️  No OpenAI key provided. You can set it later with:"
    echo "   dotnet user-secrets set \"OpenAI:ApiKey\" \"your-key-here\""
else
    dotnet user-secrets set "OpenAI:ApiKey" "$OPENAI_KEY"
    echo "✅ OpenAI API Key configured"
fi

echo ""
echo "ℹ️  Model configuration is handled in appsettings.json (not secrets)"
echo "📝 Current models: gpt-4o-mini (chat), text-embedding-3-small (embeddings)"
echo "🔧 To change models, edit appsettings.json or use appsettings.Development.json"

echo ""

echo ""
echo "ℹ️  Text Generation Provider is set to 'OpenAI' in appsettings.json"
echo "🔧 To use LLAMA instead, edit appsettings.json: \"Provider\": \"Llama\""
echo "📚 See SECRETS_GUIDE.md for more configuration options"

echo ""
echo "🎉 Configuration Complete!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# Show current secrets
echo "📋 Current User Secrets:"
dotnet user-secrets list

echo ""
echo "🚀 Ready to run! Try:"
echo "   dotnet run --urls=https://localhost:5001"
echo ""
echo "📚 For more configuration options, see SECRETS_GUIDE.md"
echo "🔍 Test your API: curl -k https://localhost:5001/api/search/health"