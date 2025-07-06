# GitHub Actions Deployment Guide

## Overview

This repository now includes updated GitHub Actions workflows for building, testing, and deploying the Contoso Travel Agents backend services to Azure. The workflows follow Azure deployment best practices and provide both full deployment and granular service-specific deployment capabilities.

## Workflows

### 1. Build and Test (`build.yaml`)

**Trigger:** 
- Push to `main` branch
- Pull requests to `main` branch
- Manual trigger via workflow_dispatch

**Purpose:** 
- Builds and tests all backend services
- Includes security scanning with Trivy
- Validates code quality across all services

**Services Built:**
- **API** (Node.js/TypeScript)
- **UI** (Angular/Node.js)
- **Customer Query** (.NET 9.0)
- **Destination Recommendation** (Java 24)
- **Echo Ping** (Node.js/TypeScript)
- **Itinerary Planning** (Python 3.13)

### 2. Azure Deploy (`azure-dev.yml`)

**Trigger:** 
- Push to `main` branch (with changes to src/, infra/, azure.yaml, or workflow files)
- Manual trigger via workflow_dispatch

**Purpose:** 
- Full infrastructure provisioning and deployment
- Builds and tests all services before deployment
- Deploys all backend services to Azure Container Apps

**Process:**
1. **Build and Test** - Matrix build for all backend services
2. **Deploy** - Provision infrastructure and deploy all services

### 3. Backend Services Deploy (`backend-services-deploy.yml`)

**Trigger:** 
- Push to `main` branch (with changes to specific service directories)
- Manual trigger with service selection

**Purpose:** 
- Granular deployment of individual services
- Intelligent change detection
- Faster deployments for single service changes

**Features:**
- **Change Detection** - Automatically detects which services have changed
- **Selective Deployment** - Deploy only the services that need updates
- **Manual Override** - Force deployment of specific services or all services
- **Post-Deployment Validation** - Verify deployment status

## Prerequisites

### Azure Configuration

1. **Service Principal/App Registration:**
   - Create an Azure App Registration with federated credentials for GitHub
   - Configure OIDC trust relationship with your GitHub repository

2. **GitHub Repository Variables:**
   Set these as repository variables (not secrets):
   ```
   AZURE_CLIENT_ID: <your-app-registration-client-id>
   AZURE_TENANT_ID: <your-azure-tenant-id>
   AZURE_SUBSCRIPTION_ID: <your-azure-subscription-id>
   AZURE_LOCATION: <your-preferred-azure-region>
   AZURE_ENV_NAME: <your-environment-name>
   ```

3. **Azure Permissions:**
   The App Registration needs the following permissions:
   - Contributor role on the target subscription or resource group
   - Permission to create resources in the specified location

### Infrastructure Setup

Ensure your `azure.yaml` and Bicep files are properly configured:

1. **azure.yaml** - Defines services and their configurations
2. **infra/main.bicep** - Infrastructure as Code definitions
3. **infra/main.parameters.json** - Environment-specific parameters

## Deployment Process

### Full Deployment

For complete infrastructure and application deployment:

1. **Automatic:** Push changes to the `main` branch
2. **Manual:** Use the "Azure Deploy" workflow with workflow_dispatch

### Service-Specific Deployment

For deploying individual services:

1. **Automatic:** Changes to service directories trigger selective deployment
2. **Manual:** Use the "Backend Services Deploy" workflow with service selection

### Manual Deployment Commands

You can also deploy manually using Azure Developer CLI:

```bash
# Full deployment
azd up

# Deploy specific service
azd deploy <service-name>

# Example: Deploy only the API service
azd deploy api
```

## Service Architecture

### Backend Services

1. **API Service** (`src/api`)
   - Node.js/TypeScript
   - Main orchestration service
   - Container App deployment

2. **Customer Query Service** (`src/tools/customer-query`)
   - .NET 9.0 MCP Server
   - Customer data queries
   - Container App deployment

3. **Destination Recommendation Service** (`src/tools/destination-recommendation`)
   - Java 24 MCP Server
   - Travel destination recommendations
   - Container App deployment

4. **Echo Ping Service** (`src/tools/echo-ping`)
   - Node.js/TypeScript MCP Server
   - Health check and testing
   - Container App deployment

5. **Itinerary Planning Service** (`src/tools/itinerary-planning`)
   - Python 3.13 MCP Server
   - Travel itinerary planning
   - Container App deployment

### Infrastructure Components

- **Azure Container Registry** - Docker image storage
- **Azure Container Apps** - Service hosting
- **Azure OpenAI** - AI/ML capabilities
- **Azure Application Insights** - Monitoring and telemetry
- **Azure Key Vault** - Secrets management

## Monitoring and Troubleshooting

### Deployment Monitoring

1. **GitHub Actions** - Monitor workflow runs in the Actions tab
2. **Azure Portal** - Check resource deployment status
3. **Azure Developer CLI** - Use `azd monitor` for telemetry

### Common Issues

1. **Authentication Failures:**
   - Verify OIDC configuration
   - Check repository variables are set correctly
   - Ensure App Registration has proper permissions

2. **Build Failures:**
   - Check service-specific build requirements
   - Verify dependency versions
   - Review build logs for specific errors

3. **Deployment Failures:**
   - Use `azd provision --preview` to validate infrastructure
   - Check Azure quotas and regional availability
   - Verify container image builds successfully

### Debugging Commands

```bash
# Check authentication
azd auth login

# Validate infrastructure
azd provision --preview

# Check deployment status
azd show

# View logs
azd logs

# Monitor services
azd monitor
```

## Security Best Practices

1. **Secrets Management:**
   - Use Azure Key Vault for application secrets
   - Never commit credentials to the repository
   - Use managed identities where possible

2. **Access Control:**
   - Implement least privilege access
   - Regular review of permissions
   - Enable Azure RBAC for fine-grained control

3. **Container Security:**
   - Regular security scanning with Trivy
   - Use official base images
   - Keep dependencies updated

4. **Network Security:**
   - Use private endpoints where applicable
   - Implement proper firewall rules
   - Enable TLS for all communications

## Contributing

When making changes to backend services:

1. Create a feature branch
2. Make your changes
3. Ensure tests pass locally
4. Create a pull request
5. Automated builds will validate your changes
6. After merge, deployment workflows will handle deployment

The workflows are designed to provide fast feedback and reliable deployments while following Azure and GitHub Actions best practices.
