# Zava Travel Agents - Deployment Guide

This guide provides step-by-step instructions for deploying the Zava Travel Agents application to Azure.

## Deployment Options

### Option 1: Azure Developer CLI (azd) - Recommended

The fastest way to deploy the application.

#### Prerequisites
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- Azure subscription

#### Steps

1. **Login to Azure**
   ```bash
   azd auth login
   ```

2. **Initialize the project**
   ```bash
   cd src/aspire/ContosoTravel.AppHost
   azd init
   ```
   
   When prompted:
   - Environment name: `zava-travel-prod`
   - Azure location: `eastus` (or your preferred region)

3. **Deploy**
   ```bash
   azd up
   ```
   
   This command will:
   - Provision Azure resources (Container Apps, Container Registry, etc.)
   - Build and push container images
   - Deploy services
   - Configure networking
   - Set up monitoring
   
   Duration: ~10-15 minutes

4. **Access your application**
   
   After deployment, azd will display the URLs:
   - Frontend: `https://travel-ui-<random>.azurecontainerapps.io`
   - Backend API: `https://chatagent-api-<random>.azurecontainerapps.io`
   - Aspire Dashboard: Available during local development

#### Update Deployment

```bash
azd up
```

#### Delete Resources

```bash
azd down --purge
```

---

### Option 2: Manual Azure Deployment

For more control over the deployment process.

## Prerequisites

- Azure subscription
- Azure CLI installed and logged in
- Docker installed (for building containers)
- Access to Azure Container Registry

## Step 1: Create Resource Group

```bash
# Set variables
RESOURCE_GROUP="rg-zava-travel"
LOCATION="eastus"
ACR_NAME="zavat
ravelacr"  # Must be globally unique

# Create resource group
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION
```

## Step 2: Create Azure Container Registry

```bash
# Create ACR
az acr create \
  --resource-group $RESOURCE_GROUP \
  --name $ACR_NAME \
  --sku Basic \
  --admin-enabled true

# Login to ACR
az acr login --name $ACR_NAME
```

## Step 3: Create Container Apps Environment

```bash
# Create Container Apps environment
az containerapp env create \
  --name zava-env \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION
```

## Step 4: Deploy Backend (Chat Agent API)

### Build and Push Container

```bash
cd src/api-dotnet/ContosoTravel.ChatAgentService/ContosoTravel.ChatAgentService

# Build and push to ACR
az acr build \
  --registry $ACR_NAME \
  --image chatagent-api:latest \
  --file Dockerfile \
  .
```

### Deploy to Container Apps

```bash
# Get ACR credentials
ACR_PASSWORD=$(az acr credential show --name $ACR_NAME --query "passwords[0].value" -o tsv)
ACR_LOGIN_SERVER="${ACR_NAME}.azurecr.io"

# Deploy container app
az containerapp create \
  --name chatagent-api \
  --resource-group $RESOURCE_GROUP \
  --environment zava-env \
  --image ${ACR_LOGIN_SERVER}/chatagent-api:latest \
  --target-port 8080 \
  --ingress external \
  --registry-server $ACR_LOGIN_SERVER \
  --registry-username $ACR_NAME \
  --registry-password "$ACR_PASSWORD" \
  --min-replicas 1 \
  --max-replicas 3 \
  --cpu 0.5 \
  --memory 1.0Gi \
  --env-vars \
    "ASPNETCORE_ENVIRONMENT=Production" \
    "AzureAI__ConnectionString=<your-connection-string>" \
    "AzureAI__AgentId=<your-agent-id>"

# Get API URL
API_URL=$(az containerapp show \
  --name chatagent-api \
  --resource-group $RESOURCE_GROUP \
  --query properties.configuration.ingress.fqdn \
  -o tsv)

echo "API URL: https://$API_URL"
```

## Step 5: Deploy Frontend (Angular UI)

### Option A: Static Web App

```bash
cd src/ui

# Build the production bundle
npm run build:production

# Create Static Web App
az staticwebapp create \
  --name zava-travel-ui \
  --resource-group $RESOURCE_GROUP \
  --source ./dist/app/browser \
  --location $LOCATION \
  --branch main \
  --app-location "/" \
  --api-location "" \
  --output-location "dist/app/browser"

# Get UI URL
UI_URL=$(az staticwebapp show \
  --name zava-travel-ui \
  --resource-group $RESOURCE_GROUP \
  --query defaultHostname \
  -o tsv)

echo "UI URL: https://$UI_URL"
```

### Option B: Container App (Alternative)

```bash
cd src/ui

# Create Dockerfile for Angular
cat > Dockerfile.production << 'EOF'
FROM node:22-alpine as build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build:production

FROM nginx:alpine
COPY --from=build /app/dist/app/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
EOF

# Build and push
az acr build \
  --registry $ACR_NAME \
  --image travel-ui:latest \
  --file Dockerfile.production \
  .

# Deploy
az containerapp create \
  --name travel-ui \
  --resource-group $RESOURCE_GROUP \
  --environment zava-env \
  --image ${ACR_LOGIN_SERVER}/travel-ui:latest \
  --target-port 80 \
  --ingress external \
  --registry-server $ACR_LOGIN_SERVER \
  --registry-username $ACR_NAME \
  --registry-password "$ACR_PASSWORD" \
  --min-replicas 1 \
  --max-replicas 5 \
  --cpu 0.25 \
  --memory 0.5Gi \
  --env-vars "API_URL=https://$API_URL"
```

## Step 6: Configure Azure AI Foundry Connection

### Create Azure AI Project

1. Go to [Azure AI Foundry](https://ai.azure.com)
2. Create a new project
3. Deploy an agent or use existing agent
4. Get the connection string and agent ID

### Update Backend Configuration

```bash
# Update environment variables
az containerapp update \
  --name chatagent-api \
  --resource-group $RESOURCE_GROUP \
  --set-env-vars \
    "AzureAI__ConnectionString=<your-connection-string>" \
    "AzureAI__AgentId=<your-agent-id>"
```

## Step 7: Configure Monitoring

### Enable Application Insights

```bash
# Create Application Insights
az monitor app-insights component create \
  --app zava-travel-insights \
  --location $LOCATION \
  --resource-group $RESOURCE_GROUP

# Get instrumentation key
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
  --app zava-travel-insights \
  --resource-group $RESOURCE_GROUP \
  --query instrumentationKey \
  -o tsv)

# Update backend with instrumentation key
az containerapp update \
  --name chatagent-api \
  --resource-group $RESOURCE_GROUP \
  --set-env-vars \
    "APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=$INSTRUMENTATION_KEY"
```

## Step 8: Configure Custom Domain (Optional)

### Backend API

```bash
# Add custom domain
az containerapp hostname add \
  --hostname api.yourdomain.com \
  --name chatagent-api \
  --resource-group $RESOURCE_GROUP

# Bind certificate
az containerapp hostname bind \
  --hostname api.yourdomain.com \
  --name chatagent-api \
  --resource-group $RESOURCE_GROUP \
  --environment zava-env \
  --validation-method HTTP
```

### Frontend

```bash
# For Static Web App
az staticwebapp hostname set \
  --name zava-travel-ui \
  --resource-group $RESOURCE_GROUP \
  --hostname www.yourdomain.com
```

## Step 9: Set Up CI/CD (Optional)

### GitHub Actions

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Azure

on:
  push:
    branches: [main]

jobs:
  deploy-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Login to Azure
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      
      - name: Build and push backend
        run: |
          az acr build \
            --registry ${{ secrets.ACR_NAME }} \
            --image chatagent-api:${{ github.sha }} \
            --file src/api-dotnet/ContosoTravel.ChatAgentService/ContosoTravel.ChatAgentService/Dockerfile \
            src/api-dotnet/ContosoTravel.ChatAgentService/ContosoTravel.ChatAgentService
      
      - name: Deploy to Container Apps
        run: |
          az containerapp update \
            --name chatagent-api \
            --resource-group ${{ secrets.RESOURCE_GROUP }} \
            --image ${{ secrets.ACR_NAME }}.azurecr.io/chatagent-api:${{ github.sha }}

  deploy-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node
        uses: actions/setup-node@v3
        with:
          node-version: '22'
      
      - name: Build frontend
        run: |
          cd src/ui
          npm ci
          npm run build:production
      
      - name: Deploy to Static Web App
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
          repo_token: ${{ secrets.GITHUB_TOKEN }}
          action: "upload"
          app_location: "src/ui"
          output_location: "dist/app/browser"
```

## Verification

### Test Backend API

```bash
curl https://$API_URL/health
```

Expected response:
```json
{
  "status": "healthy",
  "service": "chat-agent-service",
  "version": "1.0.0"
}
```

### Test Frontend

Open browser to: `https://$UI_URL`

You should see:
- Zava Travel Agents landing page
- Promotions carousel
- Floating chat button

### Test Chat Integration

1. Click the floating chat button
2. Send a message: "I want to visit Iceland"
3. Verify you receive a response

## Monitoring and Logs

### View Container Logs

```bash
# Backend logs
az containerapp logs show \
  --name chatagent-api \
  --resource-group $RESOURCE_GROUP \
  --follow

# Frontend logs (if using Container Apps)
az containerapp logs show \
  --name travel-ui \
  --resource-group $RESOURCE_GROUP \
  --follow
```

### Application Insights

Access metrics and logs at:
`https://portal.azure.com` → Application Insights → zava-travel-insights

## Scaling

### Automatic Scaling

Container Apps automatically scale based on HTTP traffic.

### Manual Scaling

```bash
# Scale backend
az containerapp update \
  --name chatagent-api \
  --resource-group $RESOURCE_GROUP \
  --min-replicas 2 \
  --max-replicas 10

# Scale frontend
az containerapp update \
  --name travel-ui \
  --resource-group $RESOURCE_GROUP \
  --min-replicas 2 \
  --max-replicas 10
```

## Cost Optimization

### Development/Staging

- Use minimum replicas: 0 or 1
- Smaller CPU/memory allocation
- Scale down during off-hours

### Production

- Enable autoscaling
- Use Azure Reserved Instances for predictable workloads
- Monitor with cost alerts

## Cleanup

### Delete All Resources

```bash
az group delete \
  --name $RESOURCE_GROUP \
  --yes \
  --no-wait
```

### Delete Individual Resources

```bash
# Delete backend
az containerapp delete \
  --name chatagent-api \
  --resource-group $RESOURCE_GROUP \
  --yes

# Delete frontend
az staticwebapp delete \
  --name zava-travel-ui \
  --resource-group $RESOURCE_GROUP \
  --yes
```

## Troubleshooting

### Container Won't Start

1. Check logs: `az containerapp logs show`
2. Verify image exists in ACR
3. Check environment variables
4. Verify port configuration

### API Not Responding

1. Check health endpoint
2. Verify ingress configuration
3. Check firewall rules
4. Review Application Insights logs

### Frontend Can't Reach API

1. Verify API_URL environment variable
2. Check CORS configuration
3. Verify network connectivity
4. Check API ingress settings

## Support Resources

- [Azure Container Apps Documentation](https://learn.microsoft.com/azure/container-apps/)
- [Azure Static Web Apps Documentation](https://learn.microsoft.com/azure/static-web-apps/)
- [Azure AI Foundry Documentation](https://learn.microsoft.com/azure/ai-studio/)
- [Aspire Deployment Guide](https://learn.microsoft.com/dotnet/aspire/deployment/overview)

## Next Steps

1. Set up monitoring dashboards
2. Configure alerts for errors/performance
3. Implement CI/CD pipeline
4. Set up staging environment
5. Configure custom domain
6. Enable Azure Front Door for CDN
7. Implement authentication (Azure AD B2C)
8. Set up backup and disaster recovery
