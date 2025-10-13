-- TripBuddy Database Schema Extension 1
-- Additional entities for enhanced functionality
-- Run this after the base schema.sql for expanded features

-- Activities that exist independently (hiking, rock climbing, etc.)
CREATE TABLE activities (
    id SERIAL PRIMARY KEY,
    nps_activity_id VARCHAR(50) UNIQUE, -- NPS activity ID
    name VARCHAR(255) NOT NULL UNIQUE,
    description TEXT,
    category VARCHAR(100), -- e.g., 'outdoor recreation', 'water sports', 'winter activities'
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Junction table linking parks to activities (many-to-many relationship)
CREATE TABLE park_activities (
    id SERIAL PRIMARY KEY,
    park_id INT REFERENCES parks(id) ON DELETE CASCADE,
    activity_id INT REFERENCES activities(id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT NOW(),
    
    UNIQUE(park_id, activity_id)
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
    
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    
    UNIQUE(park_id, nps_asset_id)
);

-- Park documents for full-text search and RAG
-- This table stores searchable content chunks that feed into the LLM for recommendations
CREATE TABLE parks_documents (
    id SERIAL PRIMARY KEY,
    
    -- Document content
    title VARCHAR(255),
    content TEXT NOT NULL, -- The actual searchable content
    content_type VARCHAR(50) NOT NULL, -- 'description', 'activities', 'features', 'weather', 'directions', 'things_to_do', 'comparison'
    
    -- Source tracking
    source_type VARCHAR(50), -- 'nps_api', 'manual', 'scraped'
    source_url TEXT, -- Original source URL if applicable
    nps_source_id VARCHAR(50), -- Reference to NPS API entity if applicable
    
    -- Search and RAG optimization
    search_vector tsvector, -- PostgreSQL full-text search
    content_hash VARCHAR(64) UNIQUE, -- MD5 hash to detect content changes
    
    -- Metadata for RAG context
    metadata JSONB, -- Flexible storage for additional context (season, difficulty, activity relevance, etc.)
    quality_score DECIMAL(3,2) DEFAULT 1.0, -- Editorial quality/source trust score (0.0-1.0)
    
    -- Content management
    is_active BOOLEAN DEFAULT true, -- Allow disabling without deletion
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Junction table linking documents to parks (many-to-many relationship)
CREATE TABLE park_document_associations (
    id SERIAL PRIMARY KEY,
    park_id INT REFERENCES parks(id) ON DELETE CASCADE,
    document_id INT REFERENCES parks_documents(id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT NOW(),
    
    UNIQUE(park_id, document_id)
);

-- Indexes for Extension 1 tables
CREATE INDEX idx_activities_nps_id ON activities(nps_activity_id);
CREATE INDEX idx_activities_name ON activities(name);
CREATE INDEX idx_activities_category ON activities(category);

CREATE INDEX idx_park_activities_park_id ON park_activities(park_id);
CREATE INDEX idx_park_activities_activity_id ON park_activities(activity_id);

CREATE INDEX idx_park_images_park_id ON park_images(park_id);
CREATE INDEX idx_park_images_type ON park_images(image_type);

CREATE INDEX idx_parks_documents_content_type ON parks_documents(content_type);
CREATE INDEX idx_parks_documents_source_type ON parks_documents(source_type);
CREATE INDEX idx_parks_documents_active ON parks_documents(is_active);
CREATE INDEX idx_parks_documents_search ON parks_documents USING gin(search_vector);
CREATE INDEX idx_parks_documents_quality ON parks_documents(quality_score);
CREATE INDEX idx_parks_documents_hash ON parks_documents(content_hash);

CREATE INDEX idx_park_document_associations_park_id ON park_document_associations(park_id);
CREATE INDEX idx_park_document_associations_document_id ON park_document_associations(document_id);

-- Full-text search for parks documents (RAG content)
CREATE INDEX idx_parks_documents_full_text ON parks_documents USING gin(
    to_tsvector('english', 
        COALESCE(title, '') || ' ' || 
        content
    )
);

-- Trigger to automatically update search_vector when parks_documents content changes
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