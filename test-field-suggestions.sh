#!/bin/bash

# Test script for AI-powered field suggestions
echo "Testing TripBuddy Field Suggestion API..."

# Create a session first
echo "1. Creating session..."
SESSION_RESPONSE=$(curl -k -s -X POST https://localhost:5001/api/sessions \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "test_user_123",
    "tripType": "backpacking",
    "initialTemplate": "backpacking-template-v1"
  }')

echo "Session response: $SESSION_RESPONSE"

# Extract session ID from response
SESSION_ID=$(echo $SESSION_RESPONSE | grep -o '"sessionId":"[^"]*"' | cut -d'"' -f4)
echo "Session ID: $SESSION_ID"

if [ -z "$SESSION_ID" ]; then
  echo "Failed to create session. Exiting."
  exit 1
fi

# Add some context to the session
echo "2. Adding context to session..."
curl -k -s -X PATCH https://localhost:5001/api/sessions/$SESSION_ID/context \
  -H "Content-Type: application/json" \
  -d '{
    "fieldUpdates": {
      "destination": "Yosemite National Park",
      "season": "spring",
      "experience_level": "intermediate"
    },
    "triggerLLM": false,
    "currentProgress": 0.3
  }' | jq '.'

# Test field suggestion for shelter
echo "3. Getting AI suggestion for shelter field..."
curl -k -s -X POST https://localhost:5001/api/sessions/$SESSION_ID/ai/field-suggestion \
  -H "Content-Type: application/json" \
  -d '{
    "fieldName": "shelter",
    "currentValue": "",
    "formContext": {
      "destination": "Yosemite National Park",
      "season": "spring",
      "experience_level": "intermediate"
    }
  }' | jq '.'

# Test field suggestion for cooking gear
echo "4. Getting AI suggestion for cooking field..."
curl -k -s -X POST https://localhost:5001/api/sessions/$SESSION_ID/ai/field-suggestion \
  -H "Content-Type: application/json" \
  -d '{
    "fieldName": "cooking",
    "currentValue": "",
    "formContext": {
      "destination": "Yosemite National Park",
      "season": "spring",
      "experience_level": "intermediate",
      "group_size": "2"
    }
  }' | jq '.'

echo "Field suggestion tests completed!"