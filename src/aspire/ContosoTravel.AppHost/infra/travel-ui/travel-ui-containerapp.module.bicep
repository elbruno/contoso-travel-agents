@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param env_outputs_azure_container_apps_environment_default_domain string

param env_outputs_azure_container_apps_environment_id string

param travel_ui_containerimage string

param appinsights_outputs_appinsightsconnectionstring string

param env_outputs_azure_container_registry_endpoint string

param env_outputs_azure_container_registry_managed_identity_id string

resource travel_ui 'Microsoft.App/containerApps@2025-01-01' = {
  name: 'travel-ui'
  location: location
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: 4200
        transport: 'tcp'
      }
      registries: [
        {
          server: env_outputs_azure_container_registry_endpoint
          identity: env_outputs_azure_container_registry_managed_identity_id
        }
      ]
    }
    environmentId: env_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: travel_ui_containerimage
          name: 'travel-ui'
          env: [
            {
              name: 'NODE_ENV'
              value: 'development'
            }
            {
              name: 'CHATAGENTSERVICE_HTTP'
              value: 'http://chatagentservice.${env_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__chatagentservice__http__0'
              value: 'http://chatagentservice.${env_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'CHATAGENTSERVICE_HTTPS'
              value: 'https://chatagentservice.${env_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__chatagentservice__https__0'
              value: 'https://chatagentservice.${env_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: appinsights_outputs_appinsightsconnectionstring
            }
            {
              name: 'NG_API_URL'
              value: 'http://chatagentservice.${env_outputs_azure_container_apps_environment_default_domain}'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${env_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}