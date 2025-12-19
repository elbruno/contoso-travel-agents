using ContosoTravel.ChatAgentService.Models;
using ContosoTravel.ChatAgentService.Services;
using Microsoft.Agents.AI.DevUI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddOpenApi();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IChatAgentService, AzureAgentService>();

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
app.MapPost("/api/agents/chat", async (ChatRequest request, IChatAgentService agentService) =>
{
    try
    {
        var response = await agentService.ProcessChatAsync(request);
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("ProcessChat")
.WithTags("Agents");

app.MapGet("/api/agents/capabilities", (IChatAgentService agentService) =>
{
    var capabilities = agentService.GetCapabilities();
    return Results.Ok(capabilities);
})
.WithName("GetCapabilities")
.WithTags("Agents");

app.MapPost("/api/agents/analyze", async (AnalyzeRequest request, IChatAgentService agentService) =>
{
    try
    {
        var result = await agentService.AnalyzeQueryAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("AnalyzeQuery")
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
