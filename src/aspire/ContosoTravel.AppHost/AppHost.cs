using Aspire.Hosting.JavaScript;

var builder = DistributedApplication.CreateBuilder(args);

// Add the chat agent backend service (.NET)
var chatAgentService = builder.AddProject<Projects.ContosoTravel_ChatAgentService>("chatagentservice")
    .WithExternalHttpEndpoints();

var chatAgentServiceHttps = chatAgentService.GetEndpoint("https");

// Register the Angular UI using the Aspire JavaScript hosting package
var angularUI = builder.AddJavaScriptApp("travel-ui", "../../ui", "start")
    // add reference to the chat agent service so it can call it directly
    .WaitFor(chatAgentService)
    .WithReference(chatAgentService)    
    // Expose external endpoints so the host can surface the UI URL
    .WithExternalHttpEndpoints()
    .WithEndpoint(port: 4200)
    // The UI expects the API url in NG_API_URL (see src/ui/.env), set that here
    .WithEnvironment("NG_API_URL", chatAgentServiceHttps);

builder.Build().Run();
