using ContosoTravel.ChatAgentService.Models;
using ContosoTravel.ChatAgentService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddSingleton<IChatAgentService, ChatAgentService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "chat-agent-service", version = "1.0.0" }))
    .WithName("HealthCheck")
    .WithTags("Health");

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

app.Run();
