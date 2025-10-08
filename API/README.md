# TripBuddy API

A .NET 8 Web API with vector search capabilities for park discovery and AI-powered recommendations.

## Architecture

- **TripBuddy.API**: Main Web API project
- **TripBuddy.Core**: Shared business logic (future)
- **TripBuddy.Infrastructure**: Data access & external APIs (future)
- **TripBuddy.Tests**: Unit tests (future)

## Quick Start

```bash
cd TripBuddy.API
dotnet restore
dotnet build
dotnet run --urls=http://localhost:5001
```

## Configuration

See `/docs/SECRETS_GUIDE.md` for API key setup.

## Documentation

- API Documentation: `/docs/API_README.md`
- Security Setup: `/docs/SECURITY_SETUP_COMPLETE.md`
- Configuration Guide: `/docs/SECRETS_GUIDE.md`
