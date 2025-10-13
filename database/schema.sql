-- TripBuddy Database Schema - Base Version
-- Simplified schema with core entities: parks and park_things_to_do
-- For additional features, run schema-extension1.sql after this file

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
    
    -- Legacy fields (keeping for compatibility)
    park_type VARCHAR(100),
    
    -- Metadata
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
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
    
    -- Full-text search optimization
    search_vector tsvector, -- Pre-computed search vector for performance
    
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    
    UNIQUE(park_id, nps_thing_id)
);

-- Note: Gear templates, categories, and items are managed as YAML files
-- This keeps the system simple and allows for easy manual editing
-- YAML files are stored in: API/TripBuddy.API/Data/GearTemplates/

-- Indexes for performance (Base Schema)
-- Parks table indexes
CREATE INDEX idx_parks_nps_park_code ON parks(nps_park_code);
CREATE INDEX idx_parks_state_code ON parks(state_code);
CREATE INDEX idx_parks_location ON parks USING gin(to_tsvector('english', name || ' ' || COALESCE(description, '') || ' ' || COALESCE(location, '')));
CREATE INDEX idx_parks_active ON parks(is_active);

-- Park things to do indexes
CREATE INDEX idx_park_things_park_id ON park_things_to_do(park_id);
CREATE INDEX idx_park_things_season ON park_things_to_do(season);
CREATE INDEX idx_park_things_tags ON park_things_to_do USING gin(activity_tags);
CREATE INDEX idx_park_things_search_vector ON park_things_to_do USING gin(search_vector);
CREATE INDEX idx_park_things_active ON park_things_to_do(is_active);
CREATE INDEX idx_park_things_duration ON park_things_to_do(duration);

-- Full-text search setup for parks
CREATE INDEX idx_parks_full_text ON parks USING gin(
    to_tsvector('english', 
        name || ' ' || 
        COALESCE(description, '') || ' ' || 
        COALESCE(full_description, '') || ' ' ||
        COALESCE(location, '')
    )
);

-- Full-text search for park_things_to_do is handled by the dedicated search_vector column and trigger

-- Trigger to automatically update search_vector for park_things_to_do when content changes
CREATE OR REPLACE FUNCTION update_park_things_search_vector() 
RETURNS trigger AS $$
BEGIN
    NEW.search_vector := to_tsvector('english', 
        NEW.title || ' ' || 
        COALESCE(NEW.short_description, '') || ' ' || 
        COALESCE(NEW.full_description, '') || ' ' ||
        COALESCE(NEW.location_description, '') || ' ' ||
        COALESCE(NEW.accessibility_info, '') || ' ' ||
        array_to_string(NEW.activity_tags, ' ')
    );
    NEW.updated_at := NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_park_things_search_vector
    BEFORE INSERT OR UPDATE ON park_things_to_do
    FOR EACH ROW
    EXECUTE FUNCTION update_park_things_search_vector();