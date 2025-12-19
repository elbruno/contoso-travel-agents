using Aspire.Hosting.JavaScript;

var builder = DistributedApplication.CreateBuilder(args);

// Add the chat agent backend service (.NET)
var chatAgentService = builder.AddProject<Projects.ContosoTravel_ChatAgentService>("chatagent-api")
    .WithExternalHttpEndpoints()
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

// Register the Angular UI using the Aspire JavaScript hosting package
var angularUI = builder.AddJavaScriptApp("travel-ui", "../../ui", "start")
    // Expose external endpoints so the host can surface the UI URL
    .WithExternalHttpEndpoints()
    // The UI expects the API url in NG_API_URL (see src/ui/.env), set that here
    .WithEnvironment("NG_API_URL", chatAgentService.GetEndpoint("http"));

builder.Build().Run();
