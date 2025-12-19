# GitHub Actions Workflows

This directory contains GitHub Actions workflows for deploying Zava Travel Agents to Azure.

## ⚠️ Important: Workflows are DISABLED by default

All deployment workflows are **disabled by default** to prevent accidental deployments. You must manually enable them before use.

## Available Workflows

### 1. Setup Azure Infrastructure (`azure-infrastructure.yml`)

**Purpose**: Creates all required Azure resources for hosting the application.

**Trigger**: Manual (workflow_dispatch) only

**What it creates**:
- Azure Resource Group
- Azure Container Registry (ACR)
- Container Apps Environment
- Application Insights
- Container Apps for API and UI (with placeholder images)

**Usage**:
1. Go to Actions → Setup Azure Infrastructure
2. Click "Run workflow"
3. Fill in the parameters:
   - Resource Group Name (default: `rg-zava-travel`)
   - Azure Location (default: `eastus`)
   - ACR Name (must be globally unique!)
4. Click "Run workflow"

**First-time setup required**:
```bash
# Create Azure service principal for GitHub Actions
az ad sp create-for-rbac \
  --name "github-actions-zava-travel" \
  --role contributor \
  --scopes /subscriptions/{subscription-id} \
  --sdk-auth

# Add the output as AZURE_CREDENTIALS secret in GitHub
```

### 2. Deploy to Azure (`azure-deploy.yml`)

**Purpose**: Builds and deploys the application to Azure Container Apps.

**Trigger**: 
- Manual (workflow_dispatch) - **ENABLED**
- Push to main branch - **DISABLED** (uncomment to enable)

**What it does**:
1. Builds the .NET backend
2. Runs tests
3. Builds Docker image and pushes to ACR
4. Deploys to Container Apps
5. Builds the Angular frontend
6. Builds Docker image and pushes to ACR
7. Deploys to Container Apps
8. Verifies deployment health

**Enable automatic deployment**:

Edit `.github/workflows/azure-deploy.yml`:

```yaml
# Change this:
if: false  # Workflow disabled by default

# To this (delete the line):
# if: false  # Workflow disabled by default
```

**Manual deployment**:
1. Go to Actions → Deploy to Azure
2. Click "Run workflow"
3. Select environment (development/staging/production)
4. Click "Run workflow"

## Required GitHub Secrets

Before running any workflow, add these secrets to your repository:

### Secrets → Actions → New repository secret

| Secret Name | Description | How to Get |
|-------------|-------------|------------|
| `AZURE_CREDENTIALS` | Azure service principal credentials | Run `az ad sp create-for-rbac` (see below) |
| `AZURE_AI_CONNECTION_STRING` | Azure AI Foundry connection string | Azure AI Foundry portal |
| `AZURE_AI_AGENT_ID` | Azure AI agent ID | Azure AI Foundry portal |
| `APPINSIGHTS_CONNECTION_STRING` | Application Insights connection | Created by infrastructure workflow |

### Getting AZURE_CREDENTIALS

```bash
# Replace {subscription-id} with your Azure subscription ID
az ad sp create-for-rbac \
  --name "github-actions-zava-travel" \
  --role contributor \
  --scopes /subscriptions/{subscription-id} \
  --sdk-auth
```

Copy the entire JSON output and add it as the `AZURE_CREDENTIALS` secret.

## Deployment Flow

### First-Time Setup

1. **Setup Azure Service Principal**
   ```bash
   az ad sp create-for-rbac \
     --name "github-actions-zava-travel" \
     --role contributor \
     --scopes /subscriptions/{subscription-id} \
     --sdk-auth
   ```

2. **Add GitHub Secrets**
   - Add `AZURE_CREDENTIALS` with service principal JSON
   - Add `AZURE_AI_CONNECTION_STRING` (from Azure AI Foundry)
   - Add `AZURE_AI_AGENT_ID` (from Azure AI Foundry)

3. **Run Infrastructure Setup**
   - Go to Actions → Setup Azure Infrastructure
   - Run workflow with your parameters
   - Note the Application Insights connection string from the output
   - Add `APPINSIGHTS_CONNECTION_STRING` as a secret

4. **Update ACR Name**
   - Edit `.github/workflows/azure-deploy.yml`
   - Change `ACR_NAME` to your unique name

5. **Enable Deployment** (optional)
   - Edit `.github/workflows/azure-deploy.yml`
   - Remove the `if: false` line

6. **Deploy Application**
   - Go to Actions → Deploy to Azure
   - Run workflow manually

### Regular Deployments

Once setup is complete, you can deploy in two ways:

**Option 1: Manual Deployment**
1. Go to Actions → Deploy to Azure
2. Click "Run workflow"
3. Select environment
4. Click "Run workflow"

**Option 2: Automatic Deployment** (if enabled)
- Push to main branch
- Workflow runs automatically

## Workflow Details

### azure-infrastructure.yml

```yaml
Inputs:
  - resource_group_name: string (default: rg-zava-travel)
  - location: choice (default: eastus)
  - acr_name: string (default: zavatravel)

Outputs:
  - Resource Group created
  - ACR credentials
  - Container Apps URLs
  - Application Insights connection string
```

### azure-deploy.yml

```yaml
Inputs:
  - environment: choice (development/staging/production)

Jobs:
  1. check-enabled: Checks if workflow is enabled
  2. build-and-deploy-backend: Builds and deploys .NET API
  3. build-and-deploy-frontend: Builds and deploys Angular UI
  4. verify-deployment: Tests deployed services

Outputs:
  - API URL
  - UI URL
  - Health check status
  - Deployment summary
```

## Environment Variables

Update these in `azure-deploy.yml` as needed:

```yaml
env:
  AZURE_RESOURCE_GROUP: rg-zava-travel
  AZURE_LOCATION: eastus
  ACR_NAME: zavatravel  # CHANGE THIS!
  UI_APP_NAME: zava-travel-ui
  API_APP_NAME: zava-chatagent-api
  CONTAINER_APPS_ENV: zava-env
```

## Monitoring Deployments

### View Workflow Runs
1. Go to Actions tab
2. Click on a workflow run
3. View logs for each job

### Check Deployment Status
- View deployment summary in workflow run
- Check Container Apps in Azure Portal
- Monitor Application Insights for telemetry

## Troubleshooting

### Infrastructure Setup Fails

**Problem**: Resource group creation fails
```
Solution: Check if resource group already exists
az group show --name rg-zava-travel
```

**Problem**: ACR name not available
```
Solution: ACR names must be globally unique. Try a different name.
```

**Problem**: Service principal doesn't have permissions
```
Solution: Ensure SP has Contributor role at subscription level
az role assignment create \
  --assignee {service-principal-id} \
  --role Contributor \
  --scope /subscriptions/{subscription-id}
```

### Deployment Fails

**Problem**: Cannot access secrets
```
Solution: Verify all secrets are added to repository:
- AZURE_CREDENTIALS
- AZURE_AI_CONNECTION_STRING
- AZURE_AI_AGENT_ID
- APPINSIGHTS_CONNECTION_STRING
```

**Problem**: ACR push fails
```
Solution: Check ACR name is correct and matches infrastructure
```

**Problem**: Container Apps update fails
```
Solution: Ensure infrastructure was created successfully
az containerapp list --resource-group rg-zava-travel
```

**Problem**: Health check fails
```
Solution: Check application logs in Azure Portal
az containerapp logs show \
  --name zava-chatagent-api \
  --resource-group rg-zava-travel \
  --follow
```

### Workflow is Disabled

**Problem**: Workflow doesn't run on push
```
Solution: This is intentional! Remove 'if: false' to enable.
```

## Cleanup

To delete all Azure resources:

```bash
az group delete \
  --name rg-zava-travel \
  --yes \
  --no-wait
```

## Best Practices

1. **Use Separate Environments**: Deploy to dev/staging/production
2. **Enable Branch Protection**: Require PR reviews before merging to main
3. **Monitor Costs**: Set up cost alerts in Azure
4. **Regular Backups**: Export configuration regularly
5. **Security Scanning**: Enable Dependabot and CodeQL
6. **Secret Rotation**: Rotate service principal credentials regularly

## Additional Workflows (Future)

Consider adding:
- **Pull Request Validation**: Build and test on PR
- **Security Scanning**: SAST/DAST scans
- **Load Testing**: Performance testing
- **Database Migrations**: Automated schema updates
- **Rollback**: Automated rollback on failure

## Support

For issues with workflows:
1. Check workflow logs in Actions tab
2. Review Azure Portal for resource status
3. Check Application Insights for runtime errors
4. Review this README for troubleshooting steps

## References

- [GitHub Actions Documentation](https://docs.github.com/actions)
- [Azure Container Apps CI/CD](https://learn.microsoft.com/azure/container-apps/github-actions)
- [Azure Service Principal](https://learn.microsoft.com/azure/developer/github/connect-from-azure)
- [GitHub Secrets](https://docs.github.com/actions/security-guides/encrypted-secrets)
