# Contoso Travel Chat Agent Service

A .NET 10 backend service providing AI chat agent capabilities for the Contoso Travel Agents application.

## Features

- **Chat Processing**: Handle user chat messages with intelligent responses
- **Query Analysis**: Analyze user queries to determine intent and extract entities
- **Agent Capabilities**: Expose available agent capabilities and supported features
- **RESTful API**: Clean HTTP endpoints for integration

## Technology Stack

- .NET 10
- ASP.NET Core Minimal APIs
- OpenAPI/Swagger

## API Endpoints

### Health Check
- `GET /health` - Service health status

### Chat Endpoints
- `POST /api/agents/chat` - Process chat messages
- `GET /api/agents/capabilities` - Get agent capabilities
- `POST /api/agents/analyze` - Analyze user queries

## Running Locally

```bash
dotnet run
```

The service will start on `http://localhost:5000`

## Building with Docker

```bash
docker build -t contoso-travel-chat-agent .
docker run -p 8080:8080 contoso-travel-chat-agent
```

## Example Requests

### Send Chat Message
```bash
curl -X POST http://localhost:5000/api/agents/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "I want to plan a trip to Iceland",
    "sessionId": "user-123"
  }'
```

### Get Capabilities
```bash
curl http://localhost:5000/api/agents/capabilities
```

### Analyze Query
```bash
curl -X POST http://localhost:5000/api/agents/analyze \
  -H "Content-Type: application/json" \
  -d '{
    "query": "What is the best time to visit Iceland?"
  }'
```

## Response Examples

### Chat Response
```json
{
  "message": "Iceland is a fantastic destination! I can help you plan a trip...",
  "sessionId": "user-123",
  "agentType": "TravelAssistant",
  "suggestions": [
    "Show me a 7-day Iceland itinerary",
    "What's the best time to visit Iceland?",
    "Budget for Iceland trip"
  ],
  "timestamp": "2025-12-19T16:00:00Z"
}
```

### Capabilities Response
```json
[
  {
    "name": "Travel Planning",
    "description": "Assists with creating personalized travel itineraries",
    "supportedLanguages": ["en", "es", "fr", "de"],
    "isAvailable": true
  }
]
```

## Development

This service is part of the Contoso Travel Agents application and integrates with the Angular frontend for providing chat agent capabilities.
