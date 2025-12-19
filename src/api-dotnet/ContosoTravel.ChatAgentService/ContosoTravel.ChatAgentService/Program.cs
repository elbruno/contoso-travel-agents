using ContosoTravel.ChatAgentService.Models;
using ContosoTravel.ChatAgentService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();

// Configure CORS based on environment
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Allow any origin in development for testing
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
    else
    {
        // Restrict to specific origins in production
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(
                      builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                      ?? new[] { "https://yourdomain.com" })
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    }
});

builder.Services.AddSingleton<IChatAgentService, AzureAgentService>();

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

app.Run();
