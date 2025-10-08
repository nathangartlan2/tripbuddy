# Database

PostgreSQL database with pgvector extension for hybrid search capabilities.

## Setup

```bash
# Install PostgreSQL
brew install postgresql

# Install pgvector extension
# See scripts/setup-postgres.sql for details
```

## Structure

```
migrations/     # Database schema migrations
seeds/         # Sample data for development
scripts/       # Setup and utility scripts
```

## Features

- **Vector Search**: pgvector extension for semantic similarity
- **Full-text Search**: PostgreSQL built-in text search
- **Geographic Search**: PostGIS for location-based queries (future)
- **Hybrid Search**: Combines vector + text + geographic search

## Tables

- `parks`: Main park data with vector embeddings
- `activities`: Park activities and features
- `user_preferences`: User search preferences (future)
- `search_history`: Query analytics (future)
