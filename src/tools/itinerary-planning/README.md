# Itinerary Planning - Python MCP Tools

## Overview

This document describes the Itinerary Planning MCP (Model Context Protocol) Tools implementation for the Contoso Travel Agents system. This Python-based solution provides comprehensive travel planning capabilities including hotel recommendations and flight suggestions with realistic mock data generation.

## Functionality

The Itinerary Planning tool serves as a comprehensive travel planning service that generates realistic travel options for customers. It provides detailed hotel and flight suggestions based on customer preferences, dates, and destinations.

### Key Features

- **Hotel Suggestions**: Generates realistic hotel recommendations with ratings, pricing, and amenities
- **Flight Suggestions**: Provides both one-way and round-trip flight options with connection handling
- **Realistic Data Generation**: Creates believable travel data using the Faker library
- **Date Validation**: Ensures proper ISO date format and logical date ordering
- **Multiple Transport Options**: Supports both direct and connecting flights
- **Comprehensive Details**: Includes pricing, availability, and service class information

## MCP Tools Published

### 1. suggest_hotels

**Name**: `suggest_hotels`  
**Description**: Suggest hotels based on location and dates.

**Parameters**:
- `location` (string): Location (city or area) to search for hotels
- `check_in` (string): Check-in date in ISO format (YYYY-MM-DD)
- `check_out` (string): Check-out date in ISO format (YYYY-MM-DD)

**Returns**: List of hotel objects containing:
- `name`: Hotel name
- `address`: Street address
- `location`: Neighborhood and city
- `rating`: Rating from 3.0 to 5.0
- `price_per_night`: Price in USD
- `hotel_type`: Type (Luxury, Boutique, Budget, Business)
- `amenities`: List of available amenities
- `available_rooms`: Number of available rooms

### 2. suggest_flights

**Name**: `suggest_flights`  
**Description**: Suggest flights based on locations and dates.

**Parameters**:
- `from_location` (string): Departure location (city or airport)
- `to_location` (string): Destination location (city or airport)
- `departure_date` (string): Departure date in ISO format (YYYY-MM-DD)
- `return_date` (string, optional): Return date in ISO format (YYYY-MM-DD)

**Returns**: Dictionary containing:
- `departure_flights`: List of outbound flight options
- `return_flights`: List of return flight options (empty for one-way)

Each flight object includes:
- `flight_id`: Unique flight identifier
- `airline`: Airline name
- `flight_number`: Flight number
- `aircraft`: Aircraft type
- `from_airport`/`to_airport`: Airport information with code, name, and city
- `departure`/`arrival`: ISO datetime strings
- `duration_minutes`: Flight duration
- `is_direct`: Boolean indicating direct vs. connecting flight
- `price`: Flight price in USD
- `available_seats`: Number of available seats
- `cabin_class`: Service class (Economy, Premium Economy, Business, First)
- `segments`: Connection details for non-direct flights (if applicable)

## Architecture Diagram

\`\`\`mermaid
graph TB
    subgraph "Client Applications"
        A[AI Assistant/Chatbot]
        B[Web Interface]
        C[Travel Agent Tools]
    end
    
    subgraph "HTTP/SSE Transport Layer"
        D[Starlette Web Framework]
        E[SSE Server Transport]
        F[HTTP Endpoints]
    end
    
    subgraph "Python MCP Server"
        G[app.py<br/>Main Application]
        H[app_routes.py<br/>Route Configuration]
        I[mcp_server.py<br/>MCP Tools]
    end
    
    subgraph "MCP Tools"
        J[suggest_hotels<br/>Hotel Recommendations]
        K[suggest_flights<br/>Flight Suggestions]
        L[validate_iso_date<br/>Date Validation]
    end
    
    subgraph "Data Generation"
        M[Faker Library<br/>Realistic Mock Data]
        N[Random Generators<br/>Prices, Times, etc.]
        O[Airport Code Generator<br/>3-Letter Codes]
    end
    
    subgraph "External Dependencies"
        P[Python 3.12+]
        Q[FastMCP Framework]
        R[Starlette/Uvicorn]
        S[Faker Library]
    end
    
    A --> D
    B --> D
    C --> D
    D --> G
    G --> H
    H --> I
    I --> J
    I --> K
    I --> L
    J --> M
    K --> M
    K --> N
    K --> O
    L --> P
    I -.-> Q
    G -.-> R
    M -.-> S
    
    classDef client fill:#e1f5fe
    classDef transport fill:#f3e5f5
    classDef server fill:#e8f5e8
    classDef tools fill:#fff3e0
    classDef data fill:#fce4ec
    classDef external fill:#f1f8e9
    
    class A,B,C client
    class D,E,F transport
    class G,H,I server
    class J,K,L tools
    class M,N,O data
    class P,Q,R,S external
\`\`\`

## Project Structure

\`\`\`
src/tools/itinerary-planning/
├── pyproject.toml                    # Python project configuration
├── uv.lock                          # Dependency lock file
├── Dockerfile                       # Container configuration
├── README.md                        # This documentation
├── src/
│   ├── app.py                       # Main application entry point
│   ├── app_routes.py                # HTTP route configuration
│   └── mcp_server.py                # MCP tools implementation
└── tests/
    ├── __init__.py                  # Test package initialization
    └── test_mcp_server.py           # Comprehensive test suite
\`\`\`

## External Components Documentation

### Python 3.12+
- **Purpose**: Base runtime environment for the application
- **Documentation**: [Python Official Documentation](https://docs.python.org/3/)
- **Key Features**: 
  - Async/await support for concurrent operations
  - Type hints for better code quality
  - Built-in datetime and regular expression support

### FastMCP Framework
- **Purpose**: Simplified MCP server creation and tool registration
- **Package**: `mcp[cli]>=1.3.0`
- **Documentation**: [MCP Python SDK](https://modelcontextprotocol.io/)
- **Key Features**:
  - Decorator-based tool definition
  - Automatic type validation
  - Built-in transport handling

### Starlette Web Framework
- **Purpose**: ASGI web framework for HTTP handling
- **Package**: `starlette>=0.46.1`
- **Documentation**: [Starlette Documentation](https://www.starlette.io/)
- **Key Features**:
  - Lightweight and fast
  - Async request handling
  - Flexible routing system

### Uvicorn ASGI Server
- **Purpose**: High-performance ASGI server for running the application
- **Package**: `uvicorn>=0.34.0`
- **Documentation**: [Uvicorn Documentation](https://www.uvicorn.org/)
- **Key Features**:
  - Fast async server implementation
  - Hot reloading for development
  - Production-ready performance

### Faker Library
- **Purpose**: Generate realistic fake data for testing and demonstrations
- **Package**: `faker>=37.1.0`
- **Documentation**: [Faker Documentation](https://faker.readthedocs.io/)
- **Key Features**:
  - Realistic address and name generation
  - Localized data support
  - Extensible provider system

### HTTPX HTTP Client
- **Purpose**: Modern async HTTP client for external API calls
- **Package**: `httpx>=0.28.1`
- **Documentation**: [HTTPX Documentation](https://www.python-httpx.org/)
- **Key Features**:
  - Async and sync API support
  - HTTP/2 support
  - Request/response middleware

## Configuration and Deployment

### Environment Requirements
- Python 3.12 or higher
- Virtual environment (recommended)
- Network access for HTTP/SSE communication
- Minimum 256MB RAM

### Local Development Setup

1. **Create virtual environment**:
   \`\`\`bash
   python -m venv venv
   source venv/bin/activate  # On Windows: venv\\Scripts\\activate
   \`\`\`

2. **Install dependencies**:
   \`\`\`bash
   pip install -e .
   \`\`\`

3. **Run the server**:
   \`\`\`bash
   python src/app.py
   \`\`\`

### Using UV Package Manager

1. **Install with UV**:
   \`\`\`bash
   uv venv
   uv pip install -e .
   \`\`\`

2. **Run with UV**:
   \`\`\`bash
   uv run src/app.py
   \`\`\`

### Testing and Development

1. **Run tests**:
   \`\`\`bash
   python -m unittest tests.test_mcp_server -v
   \`\`\`

2. **Debug with MCP Inspector**:
   \`\`\`bash
   uv run mcp dev src/mcp_server.py
   \`\`\`

## Usage Examples

### Hotel Suggestions
\`\`\`json
{
  "tool": "suggest_hotels",
  "parameters": {
    "location": "New York",
    "check_in": "2024-03-15",
    "check_out": "2024-03-17"
  }
}
\`\`\`

**Expected Response**:
\`\`\`json
[
  {
    "name": "Luxury Hotel",
    "address": "123 Broadway",
    "location": "Downtown, New York",
    "rating": 4.5,
    "price_per_night": 350,
    "hotel_type": "Luxury",
    "amenities": ["Free WiFi", "Pool", "Spa", "Gym"],
    "available_rooms": 8
  }
]
\`\`\`

### Flight Suggestions (One-way)
\`\`\`json
{
  "tool": "suggest_flights",
  "parameters": {
    "from_location": "New York",
    "to_location": "Paris",
    "departure_date": "2024-03-15"
  }
}
\`\`\`

### Flight Suggestions (Round-trip)
\`\`\`json
{
  "tool": "suggest_flights",
  "parameters": {
    "from_location": "New York",
    "to_location": "Paris",
    "departure_date": "2024-03-15",
    "return_date": "2024-03-22"
  }
}
\`\`\`

## Data Generation Logic

### Hotel Data
- **Types**: Luxury (250-600 USD), Boutique (180-350 USD), Budget (80-150 USD), Business (150-300 USD)
- **Ratings**: 3.0 to 5.0 stars
- **Amenities**: Randomly selected from pool of realistic options
- **Availability**: 1-15 rooms available
- **Sorting**: Results sorted by rating (highest first)

### Flight Data
- **Airlines**: 8 mock airlines with realistic names
- **Aircraft**: Common aircraft types (Boeing 737, Airbus A320, etc.)
- **Times**: Departures between 6 AM and 10 PM
- **Duration**: 1-8 hours for flights
- **Direct vs Connecting**: 60% direct, 40% with connections
- **Pricing**: $99-$999 USD based on route and class
- **Classes**: Economy, Premium Economy, Business, First

### Airport Codes
- **Format**: 3-letter codes following IATA convention
- **Generation**: Uses city name hints with realistic letter patterns
- **Common Hubs**: Uses real hub codes for connections (ATL, ORD, DFW, etc.)

## Performance Characteristics

- **Response Time**: Near-instantaneous for data generation
- **Throughput**: High concurrent request handling via async operations
- **Memory Usage**: Low memory footprint with efficient data structures
- **Scalability**: Horizontal scaling supported via containerization

## Error Handling

- **Date Validation**: Comprehensive ISO date format checking
- **Logical Validation**: Ensures checkout after checkin, return after departure
- **Input Sanitization**: Validates all user inputs
- **Graceful Degradation**: Handles edge cases without crashes

## Security Considerations

- Input validation for all parameters
- ISO date format enforcement
- No persistent data storage
- Stateless operation for security
- No external API dependencies (reduces attack surface)

## Future Enhancements

- Integration with real travel APIs (Amadeus, Sabre)
- Persistent booking and reservation system
- User preference learning and personalization
- Real-time pricing and availability
- Multi-currency support
- Advanced search filters and sorting options
- Integration with loyalty programs
- Weather and local events integration
