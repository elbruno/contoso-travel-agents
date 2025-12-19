using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using MEAIChatMessage = Microsoft.Extensions.AI.ChatMessage;
using MEAIChatRole = Microsoft.Extensions.AI.ChatRole;

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

// Agent endpoints - Streaming chat endpoint compatible with frontend
app.MapPost("/api/chat", async (ChatRequest request, IServiceProvider serviceProvider, HttpContext context, CancellationToken cancellationToken) =>
{
    var projectClient = serviceProvider.GetRequiredService<AIProjectClient>();
    
    context.Response.Headers.Append("Content-Type", "text/event-stream");
    context.Response.Headers.Append("Cache-Control", "no-cache");
    context.Response.Headers.Append("Connection", "keep-alive");

    try
    {
        // Get the agent - returns ChatClientAgent
        var agent = projectClient.GetAIAgent(agentName);
        
        // Create chat messages using Microsoft.Extensions.AI
        var messages = new List<MEAIChatMessage>
        {
            new(MEAIChatRole.User, request.Message)
        };
        
        // Access the underlying Client property which should be IChatClient
        var chatClient = agent.Client;
        
        // Use CompleteStreamingAsync from IChatClient
        await foreach (var update in chatClient.CompleteStreamingAsync(messages, cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                var eventData = new
                {
                    type = "metadata",
                    @event = "AgentStream",
                    data = new
                    {
                        delta = update.Text,
                        agentName = agentName
                    }
                };
                
                await context.Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(eventData)}\n\n", cancellationToken);
                await context.Response.Body.FlushAsync(cancellationToken);
            }
        }

        // Send end event
        var endEvent = new
        {
            type = "metadata",
            @event = "StopEvent",
            data = new { agentName = agentName }
        };
        await context.Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(endEvent)}\n\n", cancellationToken);
        await context.Response.Body.FlushAsync(cancellationToken);
    }
    catch (Exception ex)
    {
        var errorEvent = new
        {
            type = "error",
            message = ex.Message
        };
        await context.Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(errorEvent)}\n\n", cancellationToken);
    }
})
.WithName("StreamChat")
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

// Models for API
public record ChatRequest(string Message, List<ToolSelection>? Tools = null);
public record ToolSelection(string Id, string Name);
