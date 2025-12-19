using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddOpenApi();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// register the Microsoft Foundry Project
builder.Services.AddSingleton<AIProjectClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var projectEndpoint = configuration["AzureAI:ProjectEndpoint"];
    var tenantId = configuration["AzureAI:TenantId"];
    var agentId = configuration["AzureAI:AgentId"];

    var credentialOptions = new DefaultAzureCredentialOptions();
    if (!string.IsNullOrEmpty(tenantId))
    {
        credentialOptions = new DefaultAzureCredentialOptions()
        { TenantId = tenantId };
    }
    var tokenCredential = new DefaultAzureCredential(options: credentialOptions);

    return new AIProjectClient(
            endpoint: new Uri(projectEndpoint),
            tokenProvider: tokenCredential);
});

// register the working agent
var agentName = builder.Configuration["AzureAI:AgentName"];
builder.AddAIAgent(agentName, (sp, key) =>
{
    var projectClient = sp.GetRequiredService<AIProjectClient>();
    return projectClient.GetAIAgent(agentName);
});

// Register services for OpenAI responses and conversations (required for DevUI)
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

// Add DevUI for agent debugging and visualization
builder.AddDevUI();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapDefaultEndpoints();


// Root endpoint - return a friendly timestamp so callers can verify the backend is reachable
app.MapGet("/", () => Results.Ok(new { message = "ChatAgentService is running", timestamp = DateTime.UtcNow }))
    .WithName("Root")
    .WithTags("Info");

// Agent endpoints
app.MapPost("/api/agents/chat", async (ChatRequest request, IServiceProvider serviceProvider) =>
{
    AIAgent agent = serviceProvider.GetRequiredKeyedService<AIAgent>(agentName);
    try
    {
        var response = await agent.RunAsync(request);
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("ProcessChat")
.WithTags("Agents");


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Map DevUI endpoints for agent debugging (development only)
    app.MapOpenAIResponses();
    app.MapOpenAIConversations();
    app.MapDevUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
