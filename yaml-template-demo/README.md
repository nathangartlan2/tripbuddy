# YAML Template Demo - Trip Planning App

This demo shows how to create dynamic forms from YAML templates for trip planning applications.

## What This Demonstrates

🎯 **Core Concept**: YAML templates + React components = Dynamic trip planning forms

- YAML defines the structure and fields
- React renders the forms dynamically
- Perfect foundation for LLM integration (AI can modify YAML to add park-specific sections)

## Files Overview

### Core Files:

- **`demo.html`** - ✅ **Working standalone demo** (React + YAML parsing in browser)
- **`src/template.yaml`** - YAML template defining the backpacking trip form structure
- **`src/types.ts`** - TypeScript interfaces for type safety
- **`src/components/`** - React components for rendering forms

### Template Structure:

```yaml
trip_type: backpacking
title: "Backpacking Trip Planner"

sections:
  - id: basics
    title: "Trip Basics"
    fields:
      - name: park_name
        type: text
        label: "Park/Destination"
        required: true
```

## How to Use

### Option 1: View Working Demo (Recommended)

1. Open `demo.html` in any modern web browser
2. Fill out the dynamic form generated from YAML
3. Click "Generate Trip Plan" to see results
4. Check browser console for full form data

### Option 2: Development Setup

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Note: Currently has TypeScript compilation issues
# but demonstrates the full Vite + TypeScript approach
```

## Key Features Demonstrated

✅ **YAML to React Form Generation**

- Parses YAML template into dynamic form fields
- Supports text, number, select, and checkbox inputs
- Handles validation and required fields

✅ **Type Safety**

- TypeScript interfaces for templates and form data
- Strongly typed component props and state

✅ **Responsive UI**

- Tailwind CSS for modern styling
- Clean, mobile-friendly form layout
- Visual feedback for user interactions

✅ **Extensible Architecture**

- Easy to add new field types
- Template sections can be added/modified
- Ready for LLM integration to customize templates

## LLM Integration Strategy

This template system is designed for AI enhancement:

1. **Base Template**: Start with YAML like the backpacking template
2. **AI Customization**: LLM analyzes user's destination and adds park-specific sections:
   ```yaml
   # AI could add this section for Yosemite:
   - id: yosemite_specific
     title: "Yosemite Requirements"
     fields:
       - name: wilderness_permit
         type: checkbox
         label: "Wilderness permit obtained"
   ```
3. **Dynamic Rendering**: React components automatically render the enhanced template

## Architecture Benefits

- **Separation of Concerns**: Data (YAML) separate from presentation (React)
- **AI-Friendly**: LLMs excel at generating/modifying structured YAML
- **Type Safe**: TypeScript catches errors at development time
- **Scalable**: Easy to add new trip types, fields, or validation rules

## Next Steps for Full Implementation

1. **LLM Integration**: Add OpenAI/Anthropic API to enhance templates based on destination
2. **Backend API**: Save trip plans, retrieve park data, generate recommendations
3. **Template Library**: Multiple templates (day hiking, car camping, international travel)
4. **Smart Suggestions**: AI-powered gear recommendations based on conditions

---

🏕️ **Try the demo!** Open `demo.html` to see YAML-driven forms in action.
