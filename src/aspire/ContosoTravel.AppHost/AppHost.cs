using Aspire.Hosting.JavaScript;

var builder = DistributedApplication.CreateBuilder(args);

// Add the chat agent backend service (.NET)
var chatAgentService = builder.AddProject<Projects.ContosoTravel_ChatAgentService>("chatagent-api")
    //.WithHttpEndpoint(port: 5292, name: "http")
    .WithExternalHttpEndpoints()
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

// Register the Angular UI using the Aspire JavaScript hosting package
var angularUI = builder.AddJavaScriptApp("travel-ui", "../../ui", "start")
    .WithHttpEndpoint(port: 4200, name: "http")
    .WithEnvironment("API_URL", chatAgentService.GetEndpoint("http"))
    .WithExternalHttpEndpoints();

builder.Build().Run();
