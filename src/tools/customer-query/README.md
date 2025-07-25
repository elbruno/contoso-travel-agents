# Customer Query Analysis - .NET MCP Tools

## Overview

This document describes the Customer Query Analysis MCP (Model Context Protocol) Tools implementation for the Contoso Travel Agents system. This .NET-based solution provides intelligent analysis of customer queries to extract emotions, intents, requirements, and preferences.

## Functionality

The Customer Query Analysis tool serves as an intelligent layer between customer interactions and the travel agent system. It processes natural language customer queries and extracts structured information that can be used to provide better, more targeted assistance.

### Key Features

- **Emotion Detection**: Identifies the emotional state of customers from their queries (happy, sad, angry, neutral)
- **Intent Classification**: Determines what the customer wants to accomplish (book_flight, cancel_flight, change_flight, inquire, complaint)
- **Requirements Extraction**: Identifies travel class preferences (business, economy, first_class)
- **Preference Analysis**: Extracts seating and service preferences (window, aisle, extra_legroom)

## MCP Tools Published

### 1. CustomerQueryTool - analyze_customer_query

**Name**: `analyze_customer_query`  
**Title**: `Analyze Customer Query`  
**Description**: Analyzes the customer query and provides a response with structured information.

**Parameters**:
- `customerQuery` (string): The customer query text to analyze

**Returns**: `CustomerQueryAnalysisResult` object containing:
- `CustomerQuery`: The original query text
- `Emotion`: Detected emotional state
- `Intent`: Customer's primary intent
- `Requirements`: Travel class requirements
- `Preferences`: Seating and service preferences

### 2. EchoTool - echo

**Name**: `echo`  
**Title**: `Echo Tool`  
**Description**: Simple echo tool for testing connectivity and basic functionality.

**Parameters**:
- `message` (string): Message to echo back

**Returns**: String with "hello from .NET: {message}"

## Architecture Diagram

\`\`\`mermaid
graph TB
    subgraph "Client Applications"
        A[AI Assistant/Chatbot]
        B[Web Interface]
        C[Mobile App]
    end
    
    subgraph "MCP Transport Layer"
        D[HTTP Transport]
        E[SSE Transport]
    end
    
    subgraph "Customer Query Server (.NET)"
        F[Program.cs<br/>Main Entry Point]
        G[CustomerQueryTool<br/>MCP Tool]
        H[EchoTool<br/>MCP Tool]
    end
    
    subgraph "Core Analysis Library"
        I[CustomerQueryAnalyzer<br/>Business Logic]
        J[CustomerQueryAnalysisResult<br/>Data Model]
    end
    
    subgraph "Infrastructure"
        K[Service Defaults<br/>Telemetry & Config]
        L[Logging & Monitoring]
    end
    
    subgraph "External Dependencies"
        M[.NET Runtime 8.0]
        N[ModelContextProtocol<br/>Library]
        O[AspNetCore Framework]
    end
    
    A --> D
    B --> D
    C --> E
    D --> F
    E --> F
    F --> G
    F --> H
    G --> I
    I --> J
    F --> K
    K --> L
    F -.-> M
    G -.-> N
    F -.-> O
    
    classDef client fill:#e1f5fe
    classDef transport fill:#f3e5f5
    classDef server fill:#e8f5e8
    classDef core fill:#fff3e0
    classDef infra fill:#fce4ec
    classDef external fill:#f1f8e9
    
    class A,B,C client
    class D,E transport
    class F,G,H server
    class I,J core
    class K,L infra
    class M,N,O external
\`\`\`

## Project Structure

\`\`\`
src/tools/customer-query/
├── AITravelAgent.sln                          # Solution file
├── AITravelAgent.CustomerQueryServer/         # MCP Server hosting
│   ├── Program.cs                            # Server entry point
│   ├── Tools/
│   │   ├── CustomerQueryTool.cs              # Main analysis tool
│   │   └── EchoTool.cs                       # Simple echo tool
│   └── AITravelAgent.CustomerQueryServer.csproj
├── AITravelAgent.CustomerQueryTool/           # Core analysis library
│   ├── CustomerQueryAnalyzer.cs              # Analysis logic
│   ├── CustomerQueryAnalysisResult.cs        # Result data model
│   └── AITravelAgent.CustomerQueryTool.csproj
├── AITravelAgent.CustomerQueryTool.Tests/     # Unit tests
│   ├── Test1.cs                              # Comprehensive test suite
│   └── AITravelAgent.CustomerQueryTool.Tests.csproj
└── AITravelAgent.ServiceDefaults/             # Shared services
    ├── Extensions.cs                         # Service configuration
    └── AITravelAgent.ServiceDefaults.csproj
\`\`\`

## External Components Documentation

### .NET 8.0 Runtime
- **Purpose**: Base runtime environment for the application
- **Version**: 8.0.x
- **Documentation**: [Microsoft .NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- **Key Features**: 
  - Cross-platform compatibility
  - High performance async operations
  - Built-in dependency injection

### ModelContextProtocol Library
- **Purpose**: Enables MCP server functionality and tool registration
- **Package**: `ModelContextProtocol` and `ModelContextProtocol.AspNetCore`
- **Documentation**: [MCP Protocol Specification](https://modelcontextprotocol.io/)
- **Key Features**:
  - Automatic tool discovery and registration
  - HTTP and SSE transport support
  - Type-safe tool definitions

### ASP.NET Core Framework
- **Purpose**: Web framework for hosting the MCP server
- **Version**: 8.0.x
- **Documentation**: [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- **Key Features**:
  - Built-in HTTP server (Kestrel)
  - Middleware pipeline
  - Configuration management

### MSTest Framework
- **Purpose**: Unit testing framework for .NET applications
- **Package**: `MSTest.Sdk`
- **Version**: 3.8.3
- **Documentation**: [MSTest Documentation](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest)
- **Key Features**:
  - Attribute-based test definitions
  - Test lifecycle management
  - Assert methods for validation

## Configuration and Deployment

### Environment Requirements
- .NET 8.0 Runtime or SDK
- Windows, Linux, or macOS support
- Minimum 512MB RAM
- Network access for HTTP/SSE communication

### Build and Run Instructions

1. **Build the solution**:
   \`\`\`bash
   dotnet build
   \`\`\`

2. **Run tests**:
   \`\`\`bash
   dotnet test
   \`\`\`

3. **Start the server**:
   \`\`\`bash
   dotnet run --project AITravelAgent.CustomerQueryServer
   \`\`\`

### Configuration Options
- Port configuration via `appsettings.json`
- Logging levels and destinations
- Service discovery endpoints
- Telemetry and monitoring settings

## Usage Examples

### Basic Query Analysis
\`\`\`json
{
  "tool": "analyze_customer_query",
  "parameters": {
    "customerQuery": "I'm frustrated and need to cancel my business class flight to Paris"
  }
}
\`\`\`

**Expected Response**:
\`\`\`json
{
  "customerQuery": "I'm frustrated and need to cancel my business class flight to Paris",
  "emotion": "angry",
  "intent": "cancel_flight",
  "requirements": "business",
  "preferences": "window"
}
\`\`\`

### Echo Tool Test
\`\`\`json
{
  "tool": "echo",
  "parameters": {
    "message": "Hello MCP Server"
  }
}
\`\`\`

**Expected Response**:
\`\`\`
"hello from .NET: Hello MCP Server"
\`\`\`

## Performance Characteristics

- **Response Time**: ~1 second per analysis (includes simulated processing delay)
- **Throughput**: Designed for concurrent request handling via async/await
- **Memory Usage**: Low memory footprint with minimal allocations
- **Scalability**: Horizontal scaling supported via load balancing

## Security Considerations

- Input validation for all customer queries
- Sanitization of user-provided content
- Secure communication via HTTPS (configurable)
- No persistent storage of sensitive customer data
- Logging controls to prevent sensitive data exposure

## Future Enhancements

- Integration with actual AI/ML models for sentiment analysis
- Support for multiple languages
- Historical query pattern analysis
- Enhanced emotion classification with confidence scores
- Real-time monitoring and alerting capabilities