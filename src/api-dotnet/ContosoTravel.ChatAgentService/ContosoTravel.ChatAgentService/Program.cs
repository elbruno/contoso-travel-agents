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

// CORS: allow the UI during development to access the API from other hosts on the LAN
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        // Development convenience: allow any origin, method and header.
        // In production you should lock this down to the UI's origin(s).
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container
builder.Services.AddOpenApi();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// register the Microsoft Foundry Project
// Register AIProjectClient only if Azure AI is configured
var projectEndpoint = builder.Configuration["AzureAI:ProjectEndpoint"];
if (!string.IsNullOrEmpty(projectEndpoint))
{
    builder.Services.AddSingleton<AIProjectClient>(sp =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
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
    var agentName = builder.Configuration["AzureAI:AgentName"] ?? "travel-agent";
    builder.AddAIAgent(agentName, (sp, key) =>
    {
        var projectClient = sp.GetRequiredService<AIProjectClient>();
        return projectClient.GetAIAgent(agentName);
    });
}

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
// Use CORS policy in development so the UI can call the API when served from another host
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
}


// Root endpoint - return a friendly timestamp so callers can verify the backend is reachable
app.MapGet("/", () => Results.Ok(new { message = "ChatAgentService is running", timestamp = DateTime.UtcNow }))
    .WithName("Root")
    .WithTags("Info");

// Agent endpoints - Streaming chat endpoint compatible with frontend
app.MapPost("/api/chat", async (ChatRequest request, IServiceProvider serviceProvider, HttpContext context, CancellationToken cancellationToken) =>
{
    var agentName = app.Configuration["AzureAI:AgentName"] ?? "travel-agent";

    context.Response.Headers.Append("Content-Type", "text/event-stream");
    context.Response.Headers.Append("Cache-Control", "no-cache");
    context.Response.Headers.Append("Connection", "keep-alive");

    try
    {
        // Try to get the AI agent if configured
        AIAgent? agent = null;
        try
        {
            agent = serviceProvider.GetRequiredKeyedService<AIAgent>(agentName);
        }
        catch
        {
            // Agent not configured, send a mock response
            var mockResponse = $"🤖 Mock Agent Response: I received your message '{request.Message}'. To connect to a real Microsoft Foundry agent, configure AzureAI settings in appsettings.json.";

            var mockEventData = new
            {
                type = "metadata",
                @event = "AgentStream",
                data = new
                {
                    delta = mockResponse,
                    agentName = "mock-agent"
                }
            };


            // Send raw JSON chunk (no 'data:' prefix) so frontend can split by double-newline
            await context.Response.WriteAsync($"{System.Text.Json.JsonSerializer.Serialize(mockEventData)}\n\n", cancellationToken);
            await context.Response.Body.FlushAsync(cancellationToken);

            var mockEndEvent = new
            {
                // Frontend expects an 'end' type to signify stream completion
                type = "end",
                data = new { agentName = "mock-agent" }
            };
            await context.Response.WriteAsync($"{System.Text.Json.JsonSerializer.Serialize(mockEndEvent)}\n\n", cancellationToken);
            await context.Response.Body.FlushAsync(cancellationToken);
            return;
        }

        // Create chat messages using Microsoft.Extensions.AI
        var messages = new List<MEAIChatMessage>
        {
            new(MEAIChatRole.User, request.Message)
        };

        // Use RunStreamingAsync from AIAgent to get streaming responses
        await foreach (var update in agent.RunStreamingAsync(messages, cancellationToken: cancellationToken))
        {
            // Extract text content from the AgentRunResponseUpdate
            string? textContent = update.ToString();

            if (!string.IsNullOrEmpty(textContent))
            {
                var eventData = new
                {
                    type = "metadata",
                    @event = "AgentStream",
                    data = new
                    {
                        delta = textContent,
                        agentName = agentName
                    }
                };

                // Send raw JSON chunk (no 'data:' prefix) so frontend's parser can parse each chunk
                await context.Response.WriteAsync($"{System.Text.Json.JsonSerializer.Serialize(eventData)}\n\n", cancellationToken);
                await context.Response.Body.FlushAsync(cancellationToken);
            }
        }

        // Send end event
        var endEvent = new
        {
            // Use 'end' type to match frontend expectations for stream completion
            type = "end",
            data = new { agentName = agentName }
        };
        await context.Response.WriteAsync($"{System.Text.Json.JsonSerializer.Serialize(endEvent)}\n\n", cancellationToken);
        await context.Response.Body.FlushAsync(cancellationToken);
    }
    catch (Exception ex)
    {
        var errorEvent = new
        {
            type = "error",
            message = ex.Message
        };
        await context.Response.WriteAsync($"{System.Text.Json.JsonSerializer.Serialize(errorEvent)}\n\n", cancellationToken);
    }
})
.WithName("StreamChat")
.WithTags("Agents");


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Map DevUI endpoints for agent debugging (development only) - only if agent is configured
    if (!string.IsNullOrEmpty(projectEndpoint))
    {
        app.MapOpenAIResponses();
        app.MapOpenAIConversations();
        app.MapDevUI();
    }
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Models for API
public record ChatRequest(string Message, List<ToolSelection>? Tools = null);
public record ToolSelection(string Id, string Name);
