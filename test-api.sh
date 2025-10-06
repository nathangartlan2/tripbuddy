#!/bin/bash

# Test the TripBuddy API endpoints

BASE_URL="https://localhost:5001"

echo "=== Testing TripBuddy API ==="
echo ""

# Test health endpoint
echo "1. Testing Health Endpoint..."
curl -k -s "$BASE_URL/api/search/health" | jq '.' || echo "Health endpoint test failed"
echo ""

# Test search endpoint with a sample query
echo "2. Testing Search Endpoint..."
curl -k -s -X POST "$BASE_URL/api/search" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "I want to find parks with waterfalls and hiking trails",
    "limit": 3
  }' | jq '.' || echo "Search endpoint test failed"
echo ""

# Test search for rock climbing
echo "3. Testing Search for Rock Climbing..."
curl -k -s -X POST "$BASE_URL/api/search" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "rock climbing and granite cliffs",
    "limit": 2
  }' | jq '.' || echo "Rock climbing search test failed"
echo ""

# Test indexing a new park
echo "4. Testing Park Indexing..."
curl -k -s -X POST "$BASE_URL/api/search/index" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "test-park-1",
    "name": "Test Adventure Park",
    "description": "A test park with amazing mountain views and challenging hiking trails",
    "location": "Test Location, USA",
    "features": ["mountains", "hiking trails", "scenic views"],
    "activities": ["hiking", "photography", "camping"]
  }' | jq '.' || echo "Park indexing test failed"
echo ""

echo "=== API Testing Complete ==="