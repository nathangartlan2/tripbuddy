-- TripBuddy Database Schema
-- Enhanced schema to support NPS API data integration

-- Main parks table with enhanced NPS fields
CREATE TABLE parks (
    id SERIAL PRIMARY KEY,
    
    -- Core park information
    name VARCHAR(255) NOT NULL,
    description TEXT,
    full_description TEXT, -- Richer NPS description
    
    -- NPS-specific identifiers
    nps_park_code VARCHAR(10) UNIQUE, -- e.g., 'yose', 'grca'
    nps_designation VARCHAR(100), -- 'National Park', 'National Monument', etc.
    
    -- Location information
    location VARCHAR(255), -- General location description
    street_address TEXT,
    city VARCHAR(100),
    state_code VARCHAR(2),
    postal_code VARCHAR(10),
    latitude DECIMAL(10, 8),
    longitude DECIMAL(11, 8),
    
    -- Contact information
    phone VARCHAR(20),
    email VARCHAR(255),
    website_url TEXT,
    
    -- Operational information
    operating_hours TEXT, -- Simple text version
    hours_of_operation JSONB, -- Flexible JSON for complex schedules
    weather_info TEXT,
    directions_info TEXT,
    directions_url TEXT,
    weather_url TEXT,
    maps_url TEXT,
    
    -- Legacy fields (keeping for compatibility)
    park_type VARCHAR(100),
    features TEXT[], -- Array of feature keywords
    activities TEXT[], -- Array of activity keywords
    
    -- Metadata
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Activities available at each park (from NPS activities endpoint)
CREATE TABLE park_activities (
    id SERIAL PRIMARY KEY,
    park_id INT REFERENCES parks(id) ON DELETE CASCADE,
    nps_activity_id VARCHAR(50), -- NPS activity ID
    activity_name VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    
    UNIQUE(park_id, nps_activity_id)
);

-- Things to do at each park (from NPS thingstodo endpoint)
CREATE TABLE park_things_to_do (
    id SERIAL PRIMARY KEY,
    park_id INT REFERENCES parks(id) ON DELETE CASCADE,
    nps_thing_id VARCHAR(50), -- NPS unique ID
    title VARCHAR(255) NOT NULL,
    short_description TEXT,
    full_description TEXT,
    duration VARCHAR(100), -- e.g., "2-3 hours", "Half day"
    season VARCHAR(100), -- e.g., "Year round", "Summer only"
    activity_tags TEXT[], -- Array of related activity tags
    accessibility_info TEXT,
    fee_info TEXT,
    location_description TEXT,
    url TEXT, -- Link to more info
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    
    UNIQUE(park_id, nps_thing_id)
);

-- Park images and multimedia (from NPS multimedia endpoints)
CREATE TABLE park_images (
    id SERIAL PRIMARY KEY,
    park_id INT REFERENCES parks(id) ON DELETE CASCADE,
    nps_asset_id VARCHAR(50), -- NPS unique asset ID
    title VARCHAR(255),
    caption TEXT,
    alt_text TEXT,
    credit VARCHAR(255),
    url TEXT NOT NULL,
    
    -- Image metadata
    image_type VARCHAR(50), -- 'image', 'video', 'audio'
    file_size INT, -- in bytes
    width INT,
    height INT,
    
    created_at TIMESTAMP DEFAULT NOW(),
    
    UNIQUE(park_id, nps_asset_id)
);

-- Park documents for full-text search and RAG
-- This table stores searchable content chunks that feed into the LLM for recommendations
CREATE TABLE parks_documents (
    id SERIAL PRIMARY KEY,
    park_id INT REFERENCES parks(id) ON DELETE CASCADE,
    
    -- Document content
    title VARCHAR(255),
    content TEXT NOT NULL, -- The actual searchable content
    content_type VARCHAR(50) NOT NULL, -- 'description', 'activities', 'features', 'weather', 'directions', 'things_to_do'
    
    -- Source tracking
    source_type VARCHAR(50), -- 'nps_api', 'manual', 'scraped'
    source_url TEXT, -- Original source URL if applicable
    nps_source_id VARCHAR(50), -- Reference to NPS API entity if applicable
    
    -- Search and RAG optimization
    search_vector tsvector, -- PostgreSQL full-text search
    content_hash VARCHAR(64), -- MD5 hash to detect content changes
    
    -- Metadata for RAG context
    metadata JSONB, -- Flexible storage for additional context (season, difficulty, etc.)
    relevance_score DECIMAL(3,2) DEFAULT 1.0, -- Manual relevance weighting (0.0-1.0)
    
    -- Content management
    is_active BOOLEAN DEFAULT true, -- Allow disabling without deletion
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    
    UNIQUE(park_id, content_hash) -- Prevent duplicate content
);

-- Note: Gear templates, categories, and items are managed as YAML files
-- This keeps the system simple and allows for easy manual editing
-- YAML files are stored in: API/TripBuddy.API/Data/GearTemplates/

-- User sessions for tracking gear recommendations
CREATE TABLE user_sessions (
    id SERIAL PRIMARY KEY,
    session_token VARCHAR(255) UNIQUE NOT NULL,
    user_data JSONB, -- Flexible storage for user preferences
    
    created_at TIMESTAMP DEFAULT NOW(),
    expires_at TIMESTAMP DEFAULT (NOW() + INTERVAL '30 days')
);

-- User gear recommendations history
CREATE TABLE gear_recommendations (
    id SERIAL PRIMARY KEY,
    session_id INT REFERENCES user_sessions(id) ON DELETE CASCADE,
    park_id INT REFERENCES parks(id),
    
    -- Template reference (YAML file-based)
    gear_template_name VARCHAR(100), -- Reference to YAML file (e.g., 'backpacking', 'day_hiking')
    
    -- Recommendation context
    search_query TEXT,
    trip_duration VARCHAR(100),
    season VARCHAR(100),
    experience_level VARCHAR(50),
    
    -- Recommended gear items (denormalized from YAML)
    recommended_items JSONB, -- Complete gear list with categories from YAML processing
    
    created_at TIMESTAMP DEFAULT NOW()
);

-- Indexes for performance
CREATE INDEX idx_parks_nps_park_code ON parks(nps_park_code);
CREATE INDEX idx_parks_state_code ON parks(state_code);
CREATE INDEX idx_parks_location ON parks USING gin(to_tsvector('english', name || ' ' || COALESCE(description, '') || ' ' || COALESCE(location, '')));
CREATE INDEX idx_parks_features ON parks USING gin(features);
CREATE INDEX idx_parks_activities ON parks USING gin(activities);

CREATE INDEX idx_park_activities_park_id ON park_activities(park_id);
CREATE INDEX idx_park_activities_activity_name ON park_activities(activity_name);

CREATE INDEX idx_park_things_park_id ON park_things_to_do(park_id);
CREATE INDEX idx_park_things_season ON park_things_to_do(season);
CREATE INDEX idx_park_things_tags ON park_things_to_do USING gin(activity_tags);

CREATE INDEX idx_park_images_park_id ON park_images(park_id);
CREATE INDEX idx_park_images_type ON park_images(image_type);

CREATE INDEX idx_parks_documents_park_id ON parks_documents(park_id);
CREATE INDEX idx_parks_documents_content_type ON parks_documents(content_type);
CREATE INDEX idx_parks_documents_source_type ON parks_documents(source_type);
CREATE INDEX idx_parks_documents_active ON parks_documents(is_active);
CREATE INDEX idx_parks_documents_search ON parks_documents USING gin(search_vector);
CREATE INDEX idx_parks_documents_relevance ON parks_documents(relevance_score);

CREATE INDEX idx_user_sessions_token ON user_sessions(session_token);
CREATE INDEX idx_user_sessions_expires ON user_sessions(expires_at);

CREATE INDEX idx_gear_recommendations_session_id ON gear_recommendations(session_id);
CREATE INDEX idx_gear_recommendations_park_id ON gear_recommendations(park_id);
CREATE INDEX idx_gear_recommendations_template ON gear_recommendations(gear_template_name);
CREATE INDEX idx_gear_recommendations_created ON gear_recommendations(created_at);

-- Full-text search setup for parks
CREATE INDEX idx_parks_full_text ON parks USING gin(
    to_tsvector('english', 
        name || ' ' || 
        COALESCE(description, '') || ' ' || 
        COALESCE(full_description, '') || ' ' ||
        COALESCE(location, '') || ' ' ||
        COALESCE(weather_info, '') || ' ' ||
        array_to_string(features, ' ') || ' ' ||
        array_to_string(activities, ' ')
    )
);

-- Full-text search for things to do
CREATE INDEX idx_park_things_full_text ON park_things_to_do USING gin(
    to_tsvector('english', 
        title || ' ' || 
        COALESCE(short_description, '') || ' ' || 
        COALESCE(full_description, '') || ' ' ||
        array_to_string(activity_tags, ' ')
    )
);

-- Full-text search for parks documents (RAG content)
CREATE INDEX idx_parks_documents_full_text ON parks_documents USING gin(
    to_tsvector('english', 
        COALESCE(title, '') || ' ' || 
        content
    )
);

-- Trigger to automatically update search_vector when content changes
CREATE OR REPLACE FUNCTION update_parks_documents_search_vector() 
RETURNS trigger AS $$
BEGIN
    NEW.search_vector := to_tsvector('english', 
        COALESCE(NEW.title, '') || ' ' || NEW.content
    );
    NEW.updated_at := NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_parks_documents_search_vector
    BEFORE INSERT OR UPDATE ON parks_documents
    FOR EACH ROW
    EXECUTE FUNCTION update_parks_documents_search_vector();