namespace ContosoTravel.ChatAgentService.Models;

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public Dictionary<string, string>? Context { get; set; }
}

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string AgentType { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class AnalyzeRequest
{
    public string Query { get; set; } = string.Empty;
    public string? Language { get; set; }
}

public class AnalyzeResponse
{
    public string Intent { get; set; } = string.Empty;
    public List<string> Entities { get; set; } = new();
    public Dictionary<string, float> Confidence { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class AgentCapability
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> SupportedLanguages { get; set; } = new();
    public bool IsAvailable { get; set; } = true;
}
