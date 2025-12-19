# Zava Travel Agents - Aspire Orchestration

This directory contains the .NET Aspire orchestration for the Zava Travel Agents application.

## Overview

.NET Aspire is a cloud-ready stack for building distributed applications. It provides:
- **Orchestration**: Manages the lifecycle of application services
- **Service Discovery**: Automatic service endpoint discovery
- **Telemetry**: Built-in logging, metrics, and tracing
- **Health Checks**: Automatic health monitoring

## Architecture

The Aspire orchestration manages two main components:

1. **Chat Agent API** (.NET 10 Web API)
   - Runs on port 5292
   - Provides REST endpoints for chat functionality
   - Integrates with Azure AI Foundry agents
   
2. **Travel UI** (Angular 19)
   - Runs on port 4200
   - Modern web interface with promotions carousel
   - Floating chat window

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for containerized dependencies)
- [.NET Aspire workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling)

## Installation

### Install Aspire Workload

```bash
dotnet workload install aspire
```

### Install Dependencies

**Backend:**
```bash
cd ../../api-dotnet/ContosoTravel.ChatAgentService/ContosoTravel.ChatAgentService
dotnet restore
```

**Frontend:**
```bash
cd ../../ui
npm install
```

## Running Locally

### Using Aspire (Recommended)

From the `src/aspire/ContosoTravel.AppHost` directory:

```bash
dotnet run
```

This will:
1. Start the Chat Agent API on http://localhost:5292
2. Start the Angular UI on http://localhost:4200
3. Open the Aspire Dashboard at http://localhost:15XXX (port varies)

The Aspire Dashboard provides:
- Real-time logs from all services
- Service endpoints and health status
- Distributed tracing
- Metrics and telemetry

### Manual Start (Development)

**Terminal 1 - Backend:**
```bash
cd ../../api-dotnet/ContosoTravel.ChatAgentService/ContosoTravel.ChatAgentService
dotnet run
```

**Terminal 2 - Frontend:**
```bash
cd ../../ui
npm start
```

## Configuration

### Azure AI Foundry Connection

To connect to an existing Azure AI Foundry agent:

1. Update `appsettings.json` in the Chat Agent API:

```json
{
  "AzureAI": {
    "ConnectionString": "your-connection-string",
    "AgentId": "your-agent-id"
  }
}
```

2. Or set environment variables:

```bash
export AzureAI__ConnectionString="your-connection-string"
export AzureAI__AgentId="your-agent-id"
```

If not configured, the service runs in mock mode with simulated responses.

## Deployment to Azure

### Prerequisites

- Azure subscription
- Azure CLI installed
- Azure Developer CLI (azd) installed

### Deploy with azd

```bash
# Login to Azure
azd auth login

# Initialize the environment
azd init

# Deploy to Azure
azd up
```

This will:
1. Create Azure Container Apps environment
2. Deploy the Chat Agent API as a container
3. Deploy the Angular UI as a static web app or container
4. Configure networking and service connections
5. Set up monitoring and logging

### Manual Azure Deployment

#### 1. Create Azure Resources

```bash
# Create resource group
az group create --name rg-zava-travel --location eastus

# Create Container Apps environment
az containerapp env create \
  --name zava-env \
  --resource-group rg-zava-travel \
  --location eastus
```

#### 2. Deploy Backend

```bash
# Build and push container
cd ../../api-dotnet/ContosoTravel.ChatAgentService/ContosoTravel.ChatAgentService
az acr build --registry <your-acr> --image chatagent-api:latest .

# Deploy to Container Apps
az containerapp create \
  --name chatagent-api \
  --resource-group rg-zava-travel \
  --environment zava-env \
  --image <your-acr>.azurecr.io/chatagent-api:latest \
  --target-port 8080 \
  --ingress external \
  --env-vars "AzureAI__ConnectionString=<conn-string>" "AzureAI__AgentId=<agent-id>"
```

#### 3. Deploy Frontend

```bash
# Build Angular app
cd ../../ui
npm run build:production

# Deploy to Azure Static Web Apps
az staticwebapp create \
  --name zava-travel-ui \
  --resource-group rg-zava-travel \
  --source ./dist/app/browser \
  --location eastus
```

## Monitoring

### Aspire Dashboard

When running locally with Aspire, access the dashboard at http://localhost:15XXX

Features:
- **Resources**: View all running services
- **Console Logs**: Real-time logs from services
- **Traces**: Distributed tracing across services
- **Metrics**: Performance metrics and health

### Azure Monitor

In production, monitoring is available through:
- **Application Insights**: Telemetry and diagnostics
- **Container Apps logs**: Service logs and metrics
- **Azure Monitor**: Dashboards and alerts

## Troubleshooting

### Port Conflicts

If ports 4200 or 5292 are in use:

1. Update `AppHost.cs` to use different ports
2. Or stop conflicting services

### NPM Dependencies

If the UI fails to start:

```bash
cd ../../ui
rm -rf node_modules package-lock.json
npm install
```

### .NET Build Errors

If the backend fails to build:

```bash
cd ../../api-dotnet/ContosoTravel.ChatAgentService/ContosoTravel.ChatAgentService
dotnet clean
dotnet restore
dotnet build
```

### Azure AI Connection

If Azure AI agent is not responding:

1. Verify connection string and agent ID
2. Check agent is deployed in Azure AI Foundry
3. Verify network connectivity
4. Check service logs for authentication errors

## Project Structure

```
src/aspire/
├── ContosoTravel.AppHost/          # Aspire orchestration
│   ├── AppHost.cs                  # Service configuration
│   ├── appsettings.json            # Configuration
│   └── ContosoTravel.AppHost.csproj
├── ContosoTravel.ServiceDefaults/  # Shared service configuration
│   └── Extensions.cs               # Common extensions
└── README.md                       # This file
```

## References

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Aspire Quick Start](https://learn.microsoft.com/dotnet/aspire/get-started/build-your-first-aspire-app)
- [Azure Container Apps](https://learn.microsoft.com/azure/container-apps/)
- [Azure AI Foundry](https://learn.microsoft.com/azure/ai-studio/)
- [Microsoft Agent Framework](https://learn.microsoft.com/agent-framework/)

## Support

For issues or questions:
1. Check the [Aspire documentation](https://learn.microsoft.com/dotnet/aspire/)
2. Review logs in the Aspire Dashboard
3. Check Azure Monitor for production issues
4. Open an issue in the repository

## License

This project is licensed under the MIT License.
