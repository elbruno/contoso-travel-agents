# Zava Travel Agents - Architecture Documentation

## Overview

Zava Travel Agents is a modern, cloud-ready travel planning application built with .NET Aspire orchestration, featuring an Angular frontend and a .NET backend powered by Azure AI agents.

## High-Level Architecture

```mermaid
graph TB
    subgraph "User Layer"
        User[User Browser]
    end
    
    subgraph "Aspire Orchestration"
        AppHost[Aspire AppHost<br/>Service Orchestration]
        Dashboard[Aspire Dashboard<br/>Monitoring & Logs]
    end
    
    subgraph "Frontend Layer"
        UI[Angular UI<br/>Port 4200]
        Carousel[Promotions Carousel]
        FloatingChat[Floating Chat Window]
    end
    
    subgraph "Backend Layer"
        API[.NET 10 Web API<br/>Port 5292]
        AgentService[Azure Agent Service]
        MockMode[Mock Mode Fallback]
    end
    
    subgraph "Azure Cloud"
        AzureAI[Azure AI Foundry<br/>Agent]
        AppInsights[Application Insights<br/>Monitoring]
        ContainerApps[Azure Container Apps<br/>Hosting]
    end
    
    User --> UI
    UI --> Carousel
    UI --> FloatingChat
    FloatingChat --> API
    
    AppHost --> UI
    AppHost --> API
    AppHost --> Dashboard
    
    API --> AgentService
    AgentService --> AzureAI
    AgentService --> MockMode
    
    API --> AppInsights
    UI --> ContainerApps
    API --> ContainerApps
    
    style UI fill:#e1f5ff
    style API fill:#ffe1e1
    style AppHost fill:#e1ffe1
    style AzureAI fill:#f0e1ff
```

## Component Architecture

```mermaid
graph LR
    subgraph "Presentation Layer"
        A[Landing Page]
        B[Promotions Carousel]
        C[Floating Chat]
        D[Chat Service]
    end
    
    subgraph "API Layer"
        E[Chat Endpoint]
        F[Capabilities Endpoint]
        G[Analyze Endpoint]
        H[Health Endpoint]
    end
    
    subgraph "Business Logic"
        I[Azure Agent Service]
        J[Session Management]
        K[Intent Classification]
        L[Entity Extraction]
    end
    
    subgraph "Integration Layer"
        M[Azure AI Projects SDK]
        N[AIProjectClient]
        O[AgentsClient]
    end
    
    subgraph "External Services"
        P[Azure AI Foundry Agent]
        Q[OpenAI Models]
    end
    
    A --> B
    A --> C
    C --> D
    D --> E
    E --> I
    F --> I
    G --> I
    H --> API
    
    I --> J
    I --> K
    I --> L
    I --> M
    M --> N
    N --> O
    O --> P
    P --> Q
    
    style A fill:#b3d9ff
    style E fill:#ffb3b3
    style I fill:#b3ffb3
    style P fill:#d9b3ff
```

## Deployment Architecture

```mermaid
graph TB
    subgraph "Development"
        DevPC[Developer PC]
        AspireLocal[Aspire Local<br/>Orchestration]
        LocalUI[Angular Dev Server<br/>localhost:4200]
        LocalAPI[.NET API<br/>localhost:5292]
    end
    
    subgraph "CI/CD Pipeline"
        GitHub[GitHub Repository]
        Actions[GitHub Actions]
        Build[Build & Test]
        Deploy[Deploy to Azure]
    end
    
    subgraph "Azure Production"
        ACR[Azure Container<br/>Registry]
        CAE[Container Apps<br/>Environment]
        UIApp[UI Container App]
        APIApp[API Container App]
        AI[Azure AI Foundry]
        Monitoring[Application Insights<br/>& Log Analytics]
    end
    
    DevPC --> AspireLocal
    AspireLocal --> LocalUI
    AspireLocal --> LocalAPI
    
    DevPC --> GitHub
    GitHub --> Actions
    Actions --> Build
    Build --> Deploy
    
    Deploy --> ACR
    ACR --> CAE
    CAE --> UIApp
    CAE --> APIApp
    APIApp --> AI
    
    UIApp --> Monitoring
    APIApp --> Monitoring
    
    style DevPC fill:#e1f5ff
    style GitHub fill:#f0f0f0
    style CAE fill:#ffe1e1
    style AI fill:#f0e1ff
```

## Data Flow Architecture

```mermaid
sequenceDiagram
    participant User
    participant UI as Angular UI
    participant API as .NET API
    participant Agent as Azure Agent Service
    participant Azure as Azure AI Foundry
    participant Mock as Mock Mode
    
    User->>UI: Click Chat Button
    UI->>UI: Toggle Chat Window
    User->>UI: Enter Message
    UI->>API: POST /api/agents/chat
    API->>Agent: ProcessChatAsync(request)
    
    alt Azure AI Configured
        Agent->>Azure: CreateThread()
        Azure-->>Agent: Thread ID
        Agent->>Azure: CreateMessage(threadId, message)
        Agent->>Azure: CreateRun(threadId, agentId)
        Azure-->>Agent: Run ID
        Agent->>Azure: Poll Run Status
        Azure-->>Agent: Response
        Agent->>API: ChatResponse
    else Mock Mode
        Agent->>Mock: GenerateMockResponse()
        Mock-->>Agent: Mock Response
        Agent->>API: ChatResponse
    end
    
    API-->>UI: JSON Response
    UI-->>User: Display Message
```

## ASCII Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Zava Travel Agents                            │
│                     Cloud-Native Architecture                        │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                         USER LAYER                                   │
│                                                                       │
│   ┌─────────────┐                                                    │
│   │   Browser   │  ← User Interacts                                 │
│   └──────┬──────┘                                                    │
│          │                                                            │
└──────────┼────────────────────────────────────────────────────────-─┘
           │
           │ HTTPS
           │
┌──────────▼────────────────────────────────────────────────────────-─┐
│                      FRONTEND LAYER                                  │
│                                                                       │
│   ┌───────────────────────────────────────────┐                     │
│   │         Angular 19 UI (Port 4200)         │                     │
│   ├───────────────────────────────────────────┤                     │
│   │  ┌──────────────┐   ┌──────────────────┐ │                     │
│   │  │  Landing     │   │   Promotions     │ │                     │
│   │  │  Page        │   │   Carousel       │ │                     │
│   │  └──────────────┘   └──────────────────┘ │                     │
│   │  ┌──────────────────────────────────────┐│                     │
│   │  │    Floating Chat Window              ││                     │
│   │  │  (Bottom-Right, 400x600px)          ││                     │
│   │  └──────────────────────────────────────┘│                     │
│   └────────────────────┬──────────────────────┘                     │
│                        │                                              │
└────────────────────────┼──────────────────────────────────────────-─┘
                         │
                         │ REST API
                         │
┌────────────────────────▼──────────────────────────────────────────-─┐
│                      BACKEND LAYER                                   │
│                                                                       │
│   ┌───────────────────────────────────────────┐                     │
│   │      .NET 10 Web API (Port 5292)          │                     │
│   ├───────────────────────────────────────────┤                     │
│   │  Endpoints:                                │                     │
│   │  • POST /api/agents/chat                  │                     │
│   │  • GET  /api/agents/capabilities          │                     │
│   │  • POST /api/agents/analyze               │                     │
│   │  • GET  /health                           │                     │
│   └────────────────────┬──────────────────────┘                     │
│                        │                                              │
│   ┌────────────────────▼──────────────────────┐                     │
│   │      Azure Agent Service                  │                     │
│   ├───────────────────────────────────────────┤                     │
│   │  • Thread-Safe Session Management         │                     │
│   │  • Intent Classification                  │                     │
│   │  • Entity Extraction                      │                     │
│   │  • Dual-Mode Operation                    │                     │
│   └────────┬───────────────────┬───────────────┘                     │
│            │                   │                                      │
└────────────┼───────────────────┼──────────────────────────────────-─┘
             │                   │
             │ Connected         │ Mock
             │ Mode              │ Mode
             │                   │
┌────────────▼───────────────────▼──────────────────────────────────-─┐
│                   INTEGRATION LAYER                                  │
│                                                                       │
│   ┌─────────────────────────┐   ┌─────────────────────────┐        │
│   │  Azure AI Projects SDK  │   │   Mock Response         │        │
│   │  (v1.2.0-beta.5)       │   │   Generator             │        │
│   └──────────┬──────────────┘   └─────────────────────────┘        │
│              │                                                        │
└──────────────┼────────────────────────────────────────────────────-─┘
               │
               │ Azure SDK
               │
┌──────────────▼────────────────────────────────────────────────────-─┐
│                      AZURE CLOUD                                     │
│                                                                       │
│   ┌───────────────────────────────────────────┐                     │
│   │      Azure AI Foundry                     │                     │
│   │      ┌──────────────────────────┐         │                     │
│   │      │  AI Agent                │         │                     │
│   │      │  • GPT-4o / GPT-4        │         │                     │
│   │      │  • Thread Management     │         │                     │
│   │      │  • Tool Calling          │         │                     │
│   │      └──────────────────────────┘         │                     │
│   └───────────────────────────────────────────┘                     │
│                                                                       │
│   ┌───────────────────────────────────────────┐                     │
│   │      Azure Container Apps                 │                     │
│   │      • UI Container                       │                     │
│   │      • API Container                      │                     │
│   │      • Auto-scaling                       │                     │
│   └───────────────────────────────────────────┘                     │
│                                                                       │
│   ┌───────────────────────────────────────────┐                     │
│   │      Application Insights                 │                     │
│   │      • Distributed Tracing                │                     │
│   │      • Metrics & Logs                     │                     │
│   └───────────────────────────────────────────┘                     │
│                                                                       │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                    ORCHESTRATION LAYER                               │
│                                                                       │
│   ┌───────────────────────────────────────────┐                     │
│   │      .NET Aspire AppHost                  │                     │
│   │      • Service Discovery                  │                     │
│   │      • Health Monitoring                  │                     │
│   │      • Distributed Tracing                │                     │
│   │      • Configuration Management           │                     │
│   └───────────────────────────────────────────┘                     │
│                                                                       │
│   ┌───────────────────────────────────────────┐                     │
│   │      Aspire Dashboard                     │                     │
│   │      • Real-time Logs                     │                     │
│   │      • Service Status                     │                     │
│   │      • Resource Monitoring                │                     │
│   └───────────────────────────────────────────┘                     │
│                                                                       │
└─────────────────────────────────────────────────────────────────────┘
```

## Technology Stack

### Frontend
- **Framework**: Angular 19
- **UI Components**: Spartan UI
- **Carousel**: embla-carousel-angular 19.0.0
- **Styling**: Tailwind CSS 3.4
- **Language**: TypeScript 5.7
- **Build**: Angular CLI 19.2

### Backend
- **Framework**: .NET 10.0
- **API Style**: ASP.NET Core Minimal APIs
- **AI Integration**: Azure.AI.Projects v1.2.0-beta.5
- **Authentication**: Azure Identity
- **Language**: C# 13

### Orchestration
- **.NET Aspire**: v13.1.0
- **Service Discovery**: Built-in
- **Telemetry**: OpenTelemetry
- **Monitoring**: Aspire Dashboard

### Cloud Infrastructure
- **Hosting**: Azure Container Apps
- **AI**: Azure AI Foundry
- **Monitoring**: Application Insights
- **Registry**: Azure Container Registry
- **Deployment**: Azure Developer CLI (azd)

## Security Architecture

```mermaid
graph TB
    subgraph "Security Layers"
        A[User Authentication]
        B[CORS Policy]
        C[API Gateway]
        D[Service-to-Service Auth]
        E[Azure Managed Identity]
        F[Key Vault]
    end
    
    subgraph "Data Protection"
        G[HTTPS/TLS]
        H[Connection Strings]
        I[API Keys]
        J[Secrets Management]
    end
    
    subgraph "Monitoring"
        K[Application Insights]
        L[Security Alerts]
        M[Audit Logs]
    end
    
    A --> B
    B --> C
    C --> D
    D --> E
    E --> F
    
    F --> H
    F --> I
    F --> J
    
    G --> A
    G --> C
    
    D --> K
    K --> L
    K --> M
    
    style A fill:#ffe1e1
    style F fill:#e1ffe1
    style K fill:#e1e1ff
```

## Scalability Architecture

```mermaid
graph LR
    subgraph "Load Balancing"
        LB[Azure Load Balancer]
    end
    
    subgraph "Frontend Scale"
        UI1[UI Instance 1]
        UI2[UI Instance 2]
        UI3[UI Instance N]
    end
    
    subgraph "Backend Scale"
        API1[API Instance 1]
        API2[API Instance 2]
        API3[API Instance N]
    end
    
    subgraph "State Management"
        Cache[Redis Cache]
        Sessions[Session Storage]
    end
    
    LB --> UI1
    LB --> UI2
    LB --> UI3
    
    UI1 --> API1
    UI2 --> API2
    UI3 --> API3
    
    API1 --> Cache
    API2 --> Cache
    API3 --> Cache
    
    API1 --> Sessions
    API2 --> Sessions
    API3 --> Sessions
    
    style LB fill:#b3d9ff
    style Cache fill:#ffb3b3
```

## Network Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Internet                                  │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        │ HTTPS (443)
                        │
┌───────────────────────▼─────────────────────────────────────┐
│              Azure Front Door (Optional)                     │
│              • CDN                                           │
│              • WAF                                           │
│              • SSL/TLS Termination                          │
└───────────────────────┬─────────────────────────────────────┘
                        │
        ┌───────────────┴────────────────┐
        │                                 │
┌───────▼───────┐              ┌─────────▼────────┐
│  UI Container │              │  API Container   │
│  App          │              │  App             │
│  (Public)     │─────────────▶│  (Internal)      │
│  Port 80/443  │  HTTP/REST   │  Port 8080       │
└───────────────┘              └─────────┬────────┘
                                         │
                                         │ Azure SDK
                                         │
                              ┌──────────▼─────────┐
                              │  Azure AI Foundry  │
                              │  (Internal VNet)   │
                              └────────────────────┘
```

## Performance Characteristics

| Component | Expected Latency | Throughput | Scaling Strategy |
|-----------|-----------------|------------|------------------|
| UI Load | < 2s | N/A | CDN + Static hosting |
| API Health Check | < 100ms | 1000 req/s | Horizontal auto-scaling |
| Chat API (Mock) | < 200ms | 500 req/s | Stateless, horizontal |
| Chat API (Azure AI) | 2-5s | 100 req/s | Async processing |
| Carousel Load | < 500ms | N/A | Image CDN |

## Monitoring & Observability

```mermaid
graph TB
    subgraph "Application Layer"
        UI[Angular UI]
        API[.NET API]
    end
    
    subgraph "Telemetry Collection"
        OTEL[OpenTelemetry]
        AspireDash[Aspire Dashboard]
    end
    
    subgraph "Azure Monitoring"
        AppInsights[Application Insights]
        LogAnalytics[Log Analytics]
        Metrics[Azure Monitor Metrics]
    end
    
    subgraph "Visualization"
        Dashboards[Custom Dashboards]
        Alerts[Alert Rules]
        Workbooks[Azure Workbooks]
    end
    
    UI --> OTEL
    API --> OTEL
    OTEL --> AspireDash
    OTEL --> AppInsights
    
    AppInsights --> LogAnalytics
    AppInsights --> Metrics
    
    LogAnalytics --> Dashboards
    Metrics --> Alerts
    LogAnalytics --> Workbooks
    
    style OTEL fill:#e1f5ff
    style AppInsights fill:#ffe1e1
    style Dashboards fill:#e1ffe1
```

## Key Features

### Frontend Features
- **Responsive Design**: Mobile-first approach with Tailwind CSS
- **Component-Based**: Modular Angular components
- **Real-time Updates**: Signal-based reactivity
- **Smooth Animations**: CSS transitions and animations
- **Lazy Loading**: Route-based code splitting

### Backend Features
- **Dual-Mode Operation**: Azure AI or Mock mode
- **Thread-Safe**: Concurrent collections throughout
- **Memory-Bounded**: 50-message session limit
- **Intent Classification**: NLP-based understanding
- **Entity Extraction**: Destination recognition
- **Health Checks**: Built-in endpoints

### Aspire Features
- **Service Orchestration**: Unified lifecycle management
- **Service Discovery**: Automatic endpoint resolution
- **Distributed Tracing**: End-to-end request tracking
- **Real-time Logs**: Aggregated log viewing
- **Resource Monitoring**: CPU, memory, network metrics
- **Development Dashboard**: Local debugging interface

## Configuration

### Environment Variables

```bash
# Backend Configuration
ASPNETCORE_ENVIRONMENT=Production
AzureAI__ConnectionString=<your-connection-string>
AzureAI__AgentId=<your-agent-id>
APPLICATIONINSIGHTS_CONNECTION_STRING=<your-app-insights>

# Frontend Configuration
API_URL=https://api.yourdomain.com
```

### Ports

| Service | Port | Protocol | Access |
|---------|------|----------|--------|
| Angular UI | 4200 | HTTP | Public |
| .NET API | 5292 | HTTP | Internal |
| Aspire Dashboard | 15XXX | HTTP | Local only |
| Production UI | 80/443 | HTTPS | Public |
| Production API | 8080 | HTTP | Internal |

## References

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Azure AI Projects SDK](https://learn.microsoft.com/azure/ai-studio/)
- [Angular Documentation](https://angular.dev/)
- [Azure Container Apps](https://learn.microsoft.com/azure/container-apps/)
