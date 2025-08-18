# Diagramas de Arquitectura MCP - Contoso Travel Agents

Este documento complementa la documentación principal con diagramas detallados de la arquitectura de los servidores MCP.

## Arquitectura Detallada del Sistema MCP

### Vista General de Componentes

```mermaid
graph TB
    subgraph "Cliente Layer"
        A[AI Travel Agent Client]
        B[MCP Client SDK]
    end
    
    subgraph "Transport Layer"
        C[HTTP/REST API]
        D[Server-Sent Events]
        E[MCP Protocol Handler]
    end
    
    subgraph "Servidor .NET - Customer Query"
        F[ASP.NET Core Host]
        G[MCP.AspNetCore Middleware]
        H[CustomerQueryTool]
        I[CustomerQueryAnalyzer]
        J[ServiceDefaults]
    end
    
    subgraph "Servidor Python - Itinerary Planning"
        K[Starlette App]
        L[FastMCP Framework]
        M[Hotel Suggestion Tool]
        N[Flight Suggestion Tool]
        O[Date Validation]
    end
    
    subgraph "Observabilidad"
        P[OpenTelemetry]
        Q[Métricas]
        R[Trazas Distribuidas]
        S[Health Checks]
    end
    
    A --> B
    B --> C
    B --> D
    C --> E
    D --> E
    
    E --> F
    E --> K
    
    F --> G
    G --> H
    H --> I
    F --> J
    
    K --> L
    L --> M
    L --> N
    M --> O
    N --> O
    
    F --> P
    K --> P
    P --> Q
    P --> R
    J --> S
    
    style A fill:#e3f2fd
    style F fill:#f3e5f5
    style K fill:#e8f5e8
    style P fill:#fff3e0
```

## Flujo de Datos del Customer Query (.NET)

### Procesamiento de Consulta de Cliente

```mermaid
sequenceDiagram
    participant C as Cliente MCP
    participant MW as MCP Middleware
    participant CT as CustomerQueryTool
    participant CA as CustomerQueryAnalyzer
    participant OT as OpenTelemetry
    
    C->>MW: POST /mcp/call {analyze_customer_query}
    MW->>OT: Iniciar span "tool_call_analyze_customer_query"
    MW->>CT: AnalyzeCustomerQueryAsync(query)
    
    CT->>OT: Log "Received customer query"
    CT->>CA: AnalyzeAsync(customerQuery)
    
    CA->>CA: Task.Delay(1000) // Simulación
    CA->>CA: Análisis probabilístico
    
    Note over CA: Selección aleatoria de:<br/>- Emociones: happy, sad, angry, neutral<br/>- Intenciones: book_flight, cancel_flight, etc.<br/>- Requisitos: business, economy, first_class<br/>- Preferencias: window, aisle, extra_legroom
    
    CA-->>CT: CustomerQueryAnalysisResult
    CT->>OT: Log "Analysis result"
    CT-->>MW: CustomerQueryAnalysisResult
    MW->>OT: Finalizar span (Ok)
    MW-->>C: HTTP 200 + ToolCallResponse
```

## Flujo de Datos del Itinerary Planning (Python)

### Sugerencia de Hoteles

```mermaid
sequenceDiagram
    participant C as Cliente MCP
    participant S as Starlette App
    participant F as FastMCP
    participant H as suggest_hotels
    participant V as validate_iso_date
    participant FK as Faker
    
    C->>S: POST /mcp/call {suggest_hotels}
    S->>F: Enrutar herramienta
    F->>H: suggest_hotels(location, check_in, check_out)
    
    H->>V: validate_iso_date(check_in)
    V->>V: Validar formato YYYY-MM-DD
    V->>V: datetime.strptime()
    V-->>H: date object
    
    H->>V: validate_iso_date(check_out)
    V-->>H: date object
    
    H->>H: Verificar check_out > check_in
    
    loop Generar 3-8 hoteles
        H->>FK: fake.street_address()
        H->>H: Generar rating (3.0-5.0)
        H->>H: Calcular precio por tipo
        H->>H: Seleccionar amenidades
        FK-->>H: Datos realistas
    end
    
    H->>H: Ordenar por rating DESC
    H-->>F: Lista de hoteles
    F-->>S: Respuesta JSON
    S-->>C: HTTP 200 + hoteles
```

### Sugerencia de Vuelos

```mermaid
sequenceDiagram
    participant C as Cliente MCP
    participant F as FastMCP
    participant SF as suggest_flights
    participant V as validate_iso_date
    participant G as Flight Generator
    
    C->>F: suggest_flights(from, to, dep_date, ret_date?)
    F->>SF: Procesar solicitud
    
    SF->>V: validate_iso_date(departure_date)
    V-->>SF: departure_date
    
    opt Si return_date existe
        SF->>V: validate_iso_date(return_date)
        V-->>SF: return_date
        SF->>SF: Verificar return > departure
    end
    
    SF->>G: Generar vuelos de ida (3-7)
    
    loop Para cada vuelo
        G->>G: Generar horario (06:00-22:00)
        G->>G: Calcular duración (1-8 horas)
        G->>G: Decidir directo vs conexión (60%/40%)
        
        alt Vuelo directo
            G->>G: Vuelo simple
        else Vuelo con conexión
            G->>G: Generar segmentos
            G->>G: Calcular tiempo de conexión
        end
        
        G->>G: Asignar aerolínea, precio, asientos
    end
    
    opt Si return_date existe
        SF->>G: Generar vuelos de vuelta
    end
    
    SF-->>F: {departure_flights: [], return_flights: []}
    F-->>C: Respuesta de vuelos
```

## Arquitectura de Pruebas

### Estructura de Pruebas .NET

```mermaid
graph TD
    subgraph "Proyecto de Pruebas"
        A[AITravelAgent.CustomerQueryTool.Tests]
    end
    
    subgraph "Casos de Prueba"
        B[CustomerQueryAnalyzerTests]
        C[CustomerQueryAnalysisResultTests]
    end
    
    subgraph "Tipos de Prueba"
        D[Pruebas de Unidad]
        E[Pruebas de Integración]
        F[Pruebas de Validación]
        G[Pruebas de Rendimiento]
    end
    
    subgraph "Framework"
        H[MSTest]
        I[Assert Methods]
        J[TestInitialize/Cleanup]
    end
    
    A --> B
    A --> C
    B --> D
    B --> F
    B --> G
    C --> D
    C --> F
    
    B --> H
    C --> H
    H --> I
    H --> J
    
    style A fill:#e3f2fd
    style B fill:#f3e5f5
    style H fill:#fff3e0
```

### Estructura de Pruebas Python

```mermaid
graph TD
    subgraph "Directorio de Pruebas"
        A[tests/]
    end
    
    subgraph "Archivos de Prueba"
        B[test_mcp_server.py]
        C[test_app.py]
        D[__init__.py]
    end
    
    subgraph "Clases de Prueba"
        E[TestValidateIsoDate]
        F[TestSuggestHotels]
        G[TestSuggestFlights]
        H[TestApp]
    end
    
    subgraph "Framework y Herramientas"
        I[pytest]
        J[pytest-asyncio]
        K[pytest-cov]
        L[TestClient]
    end
    
    A --> B
    A --> C
    A --> D
    
    B --> E
    B --> F
    B --> G
    C --> H
    
    E --> I
    F --> J
    G --> J
    H --> L
    
    I --> K
    
    style A fill:#e8f5e8
    style B fill:#f3e5f5
    style I fill:#fff3e0
```

## Diagrama de Despliegue

### Contenedorización y Orquestación

```mermaid
graph TB
    subgraph "Docker Environment"
        subgraph "Customer Query Container"
            A[.NET 8.0 Runtime]
            B[CustomerQueryServer]
            C[Port 5001:8080]
        end
        
        subgraph "Itinerary Planning Container"
            D[Python 3.12]
            E[Uvicorn Server]
            F[Port 8000]
        end
        
        subgraph "Shared Services"
            G[OpenTelemetry Collector]
            H[Service Discovery]
            I[Load Balancer]
        end
    end
    
    subgraph "External Dependencies"
        J[Azure AI Services]
        K[Aspire Dashboard]
        L[External APIs]
    end
    
    B --> C
    E --> F
    
    B --> G
    E --> G
    
    B --> H
    E --> H
    
    I --> B
    I --> E
    
    B --> J
    G --> K
    
    style A fill:#f3e5f5
    style D fill:#e8f5e8
    style G fill:#fff3e0
```

## Flujo de Observabilidad

### Trazas Distribuidas

```mermaid
sequenceDiagram
    participant C as Cliente
    participant LB as Load Balancer
    participant CS as Customer Service
    participant IP as Itinerary Planning
    participant OT as OpenTelemetry
    participant AD as Aspire Dashboard
    
    C->>LB: Solicitud de viaje
    LB->>CS: analyze_customer_query
    CS->>OT: Crear span raíz
    
    CS->>CS: Procesar consulta
    CS->>OT: Métricas de procesamiento
    
    CS->>IP: suggest_hotels
    IP->>OT: Crear span hijo
    IP->>IP: Generar hoteles
    IP->>OT: Métricas de generación
    IP-->>CS: Lista de hoteles
    
    CS->>IP: suggest_flights
    IP->>OT: Crear span hijo
    IP->>IP: Generar vuelos
    IP-->>CS: Lista de vuelos
    
    CS-->>LB: Respuesta completa
    LB-->>C: Resultado final
    
    OT->>AD: Enviar trazas y métricas
    
    Note over OT,AD: Visualización en tiempo real<br/>de rendimiento y errores
```

## Matriz de Compatibilidad

### Versiones y Dependencias

```mermaid
graph LR
    subgraph ".NET Ecosystem"
        A[.NET 8.0 SDK]
        B[ASP.NET Core 8.0]
        C[ModelContextProtocol 0.*]
        D[OpenTelemetry 1.9.0]
    end
    
    subgraph "Python Ecosystem"
        E[Python 3.12+]
        F[Starlette 0.46.1+]
        G[MCP 1.3.0+]
        H[Faker 37.1.0+]
    end
    
    subgraph "Testing Frameworks"
        I[MSTest 3.8.3]
        J[pytest 8.0.0+]
        K[pytest-asyncio 0.21.0+]
    end
    
    subgraph "Infrastructure"
        L[Docker]
        M[Docker Compose]
        N[OpenTelemetry Collector]
    end
    
    A --> B
    B --> C
    C --> D
    
    E --> F
    F --> G
    G --> H
    
    A --> I
    E --> J
    J --> K
    
    B --> L
    F --> L
    L --> M
    D --> N
    
    style A fill:#f3e5f5
    style E fill:#e8f5e8
    style L fill:#fff3e0
```

---

**Nota**: Estos diagramas representan la arquitectura actual implementada y validada mediante las pruebas unitarias desarrolladas. Para actualizaciones o modificaciones, consulte la documentación principal en `README.md`.