var builder = DistributedApplication.CreateBuilder(args);

// Add the chat agent backend service (.NET)
var chatAgentService = builder.AddProject<Projects.ContosoTravel_ChatAgentService>("chatagent-api")
    .WithHttpEndpoint(port: 5292, name: "http")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

// Add the Angular UI as an npm app
var angularUI = builder.AddNpmApp("travel-ui", "../../ui", "start")
    .WithHttpEndpoint(port: 4200, name: "http")
    .WithEnvironment("API_URL", chatAgentService.GetEndpoint("http"))
    .WithExternalHttpEndpoints();

builder.Build().Run();
