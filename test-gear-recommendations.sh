#!/bin/bash

# Test script for AI-powered gear recommendations
echo "🎒 Testing TripBuddy Gear Recommendation API..."

API_BASE="https://localhost:5001/api/gear"

echo "1. Health check..."
curl -k -s "$API_BASE/health" | jq '.'

echo -e "\n2. Get base backpacking template..."
curl -k -s "$API_BASE/base-template/backpacking" | jq '.categories[0]'

echo -e "\n3. Generate custom gear list for Yosemite winter backpacking..."
curl -k -s -X POST "$API_BASE/recommendations" \
  -H "Content-Type: application/json" \
  -d '{
    "tripContext": {
      "destination": "Yosemite National Park",
      "season": "winter",
      "duration": "3 days",
      "experienceLevel": "intermediate",
      "groupSize": 2,
      "tripType": "backpacking"
    }
  }' | jq '.'

echo -e "\n4. Generate custom gear list for Grand Canyon summer day hiking..."
curl -k -s -X POST "$API_BASE/recommendations" \
  -H "Content-Type: application/json" \
  -d '{
    "tripContext": {
      "destination": "Grand Canyon",
      "season": "summer",
      "duration": "1 day",
      "experienceLevel": "beginner",
      "groupSize": 4,
      "tripType": "day_hiking"
    }
  }' | jq '.'

echo -e "\n5. Generate custom gear list for Yellowstone car camping..."
curl -k -s -X POST "$API_BASE/recommendations" \
  -H "Content-Type: application/json" \
  -d '{
    "tripContext": {
      "destination": "Yellowstone National Park",
      "season": "fall",
      "duration": "1 week",
      "experienceLevel": "expert",
      "groupSize": 6,
      "tripType": "car_camping"
    }
  }' | jq '.gearList.items[] | select(.modification != null) | {name: .name, category: .category, note: .note}'

echo -e "\nGear recommendation tests completed! 🎉"