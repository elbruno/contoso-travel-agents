using Aspire.Hosting.JavaScript;

var builder = DistributedApplication.CreateBuilder(args);

// Add the following line to configure the Azure App Container environment
builder.AddAzureContainerAppEnvironment("env");


// Application Insights for telemetry
IResourceBuilder<IResourceWithConnectionString>? appInsights;

if (builder.ExecutionContext.IsPublishMode)
{
    // PRODUCTION: Use Azure-provisioned services
    appInsights = builder.AddAzureApplicationInsights("appInsights");
}
else
{
    // DEVELOPMENT: Use connection strings from configuration
    appInsights = builder.AddConnectionString("appinsights", "APPLICATIONINSIGHTS_CONNECTION_STRING");
}

// Add the chat agent backend service (.NET)
var chatAgentService = builder.AddProject<Projects.ContosoTravel_ChatAgentService>("chatagentservice")
    .WithReference(appInsights) // Add reference to Application Insights for telemetry
    .WithExternalHttpEndpoints();
var chatAgentServiceHttp = chatAgentService.GetEndpoint("http");
var chatAgentServiceHttps = chatAgentService.GetEndpoint("https");


// Register the Angular UI using the Aspire JavaScript hosting package
var angularUI = builder.AddJavaScriptApp("travel-ui", "../../ui", "start")
    // add reference to the chat agent service so it can call it directly
    .WaitFor(chatAgentService)
    .WithReference(chatAgentService)
    // Add reference to Application Insights for telemetry
    .WithReference(appInsights) 
    // Expose external endpoints so the host can surface the UI URL
    .WithExternalHttpEndpoints()
    .WithEndpoint(port: 4200)
    // The UI expects the API url in NG_API_URL (see src/ui/.env), set that here
    .WithEnvironment("NG_API_URL", chatAgentServiceHttp);

builder.Build().Run();
