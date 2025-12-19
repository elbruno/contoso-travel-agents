using './travel-ui-containerapp.module.bicep'

param appinsights_outputs_appinsightsconnectionstring = '{{ .Env.APPINSIGHTS_APPINSIGHTSCONNECTIONSTRING }}'
param env_outputs_azure_container_apps_environment_default_domain = '{{ .Env.ENV_AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN }}'
param env_outputs_azure_container_apps_environment_id = '{{ .Env.ENV_AZURE_CONTAINER_APPS_ENVIRONMENT_ID }}'
param env_outputs_azure_container_registry_endpoint = '{{ .Env.ENV_AZURE_CONTAINER_REGISTRY_ENDPOINT }}'
param env_outputs_azure_container_registry_managed_identity_id = '{{ .Env.ENV_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID }}'
param travel_ui_containerimage = '{{ .Image }}'
