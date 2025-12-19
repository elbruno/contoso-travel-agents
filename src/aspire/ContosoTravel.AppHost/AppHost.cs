var builder = DistributedApplication.CreateBuilder(args);

// Add the chat agent backend service (.NET)
var chatAgentService = builder.AddProject<Projects.ContosoTravel_ChatAgentService>("chatagent-api")
    //.WithHttpEndpoint(port: 5292, name: "http")
    .WithExternalHttpEndpoints()
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

// Angular UI registration is disabled in this build environment because the AddNpmApp extension
// is not available in the current Aspire SDK. The UI can be run separately during development.
var angularUI = builder.AddNpmApp("travel-ui", "../../ui", "start")
    .WithHttpEndpoint(port: 4200, name: "http")
    .WithEnvironment("API_URL", chatAgentService.GetEndpoint("http"))
    .WithExternalHttpEndpoints();

builder.Build().Run();
