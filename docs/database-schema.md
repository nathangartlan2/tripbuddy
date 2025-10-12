# TripBuddy Database Schema

## Entity Relationship Diagram

```mermaid
erDiagram
    parks {
        int id PK
        varchar name
        text description
        text full_description
        varchar nps_park_code UK
        varchar nps_designation
        varchar location
        text street_address
        varchar city
        varchar state_code
        varchar postal_code
        decimal latitude
        decimal longitude
        varchar phone
        varchar email
        text website_url
        text operating_hours
        jsonb hours_of_operation
        text weather_info
        text directions_info
        text directions_url
        text weather_url
        text maps_url
        varchar park_type
        text_array features
        text_array activities
        timestamp created_at
        timestamp updated_at
    }

    park_activities {
        int id PK
        int park_id FK
        varchar nps_activity_id
        varchar activity_name
        timestamp created_at
    }

    park_things_to_do {
        int id PK
        int park_id FK
        varchar nps_thing_id
        varchar title
        text short_description
        text full_description
        varchar duration
        varchar season
        text_array activity_tags
        text accessibility_info
        text fee_info
        text location_description
        text url
        timestamp created_at
        timestamp updated_at
    }

    park_images {
        int id PK
        int park_id FK
        varchar nps_asset_id
        varchar title
        text caption
        text alt_text
        varchar credit
        text url
        varchar image_type
        int file_size
        int width
        int height
        timestamp created_at
    }

    parks_documents {
        int id PK
        int park_id FK
        varchar title
        text content
        varchar content_type
        varchar source_type
        text source_url
        varchar nps_source_id
        tsvector search_vector
        varchar content_hash
        jsonb metadata
        decimal relevance_score
        boolean is_active
        timestamp created_at
        timestamp updated_at
    }

    %% Relationships
    parks ||--o{ park_activities : "has"
    parks ||--o{ park_things_to_do : "has"
    parks ||--o{ park_images : "has"
    parks ||--o{ parks_documents : "has"

    %% External Reference (YAML Files)
    gear_yaml_files {
        string filename
        string trip_type
        array categories
        array items
    }
```

## Key Features

### **Core Entities**

- **parks**: Main park information with NPS API integration
- **parks_documents**: RAG content for LLM context

### **NPS API Integration**

- **park_activities**: Activities available at each park
- **park_things_to_do**: Detailed activity descriptions
- **park_images**: Multimedia content from NPS

### **External References**

- **Gear Templates**: Managed as YAML files (not in database)
  - `backpacking.yaml`
  - `car_camping.yaml`
  - `day_hiking.yaml`

### **Search Capabilities**

- **Full-text search** on parks and documents using PostgreSQL `tsvector`
- **Keyword search** for RAG content retrieval
- **Metadata** in JSONB for flexible queries

### **Simplified Architecture**

This schema focuses purely on:

1. **Park Data Storage** - Rich NPS API integration
2. **Content Management** - RAG-optimized document storage
3. **Search Performance** - Full-text search indexes

### **Application Layer Responsibilities**

- **Gear Recommendations**: Handled via YAML templates + stateless API
- **User Sessions**: Managed in application memory or external cache
- **Recommendation History**: Optional future enhancement
