using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using ContosoTravel.ChatAgentService.Models;

namespace ContosoTravel.ChatAgentService.Services;

public class AzureAgentService : IChatAgentService
{
    private readonly ILogger<AzureAgentService> _logger;
    private readonly IConfiguration _configuration;
    private AIProjectClient? _projectClient;
    private string? _agentId;

    public AzureAgentService(ILogger<AzureAgentService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        InitializeAgent();
    }

    private void InitializeAgent()
    {
        try
        {
            var connectionString = _configuration["AzureAI:ConnectionString"];
            var agentId = _configuration["AzureAI:AgentId"];

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogWarning("Azure AI connection string not configured. Agent will run in mock mode.");
                return;
            }

            _projectClient = new AIProjectClient(connectionString, new DefaultAzureCredential());
            _agentId = agentId;
            
            _logger.LogInformation("Azure AI Agent initialized successfully with agent ID: {AgentId}", agentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Azure AI Agent. Running in mock mode.");
        }
    }

    public async Task<ChatResponse> ProcessChatAsync(ChatRequest request)
    {
        _logger.LogInformation("Processing chat request: {Message}", request.Message);

        var sessionId = request.SessionId ?? Guid.NewGuid().ToString();

        try
        {
            if (_projectClient != null && !string.IsNullOrEmpty(_agentId))
            {
                // Use Azure AI Agent
                return await ProcessWithAzureAgentAsync(request, sessionId);
            }
            else
            {
                // Fallback to mock responses
                return ProcessWithMockAgent(request, sessionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
            return new ChatResponse
            {
                SessionId = sessionId,
                AgentType = "Error",
                Message = "I apologize, but I encountered an error processing your request. Please try again.",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    private async Task<ChatResponse> ProcessWithAzureAgentAsync(ChatRequest request, string sessionId)
    {
        // Get the agents client from project
        var agentsClient = _projectClient!.GetAgentsClient();
        
        // Create a thread for conversation
        var threadResponse = await agentsClient.CreateThreadAsync();
        var threadId = threadResponse.Value.Id;

        // Add user message to thread
        await agentsClient.CreateMessageAsync(
            threadId,
            MessageRole.User,
            request.Message);

        // Run the agent
        var runResponse = await agentsClient.CreateRunAsync(threadId, _agentId!);
        var run = runResponse.Value;

        // Wait for completion
        while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress)
        {
            await Task.Delay(1000);
            run = (await agentsClient.GetRunAsync(threadId, run.Id)).Value;
        }

        // Get the assistant's response
        var messages = await agentsClient.GetMessagesAsync(threadId);
        var assistantMessage = messages.Value.Data.FirstOrDefault(m => m.Role == MessageRole.Assistant);

        var responseText = assistantMessage?.ContentItems
            .OfType<MessageTextContent>()
            .FirstOrDefault()?.Text ?? "I don't have a response at this time.";

        return new ChatResponse
        {
            SessionId = sessionId,
            AgentType = "AzureAIAgent",
            Message = responseText,
            Suggestions = GenerateSuggestions(request.Message),
            Timestamp = DateTime.UtcNow
        };
    }

    private ChatResponse ProcessWithMockAgent(ChatRequest request, string sessionId)
    {
        return new ChatResponse
        {
            SessionId = sessionId,
            AgentType = "MockAgent",
            Message = GenerateMockResponse(request.Message),
            Suggestions = GenerateSuggestions(request.Message),
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<AnalyzeResponse> AnalyzeQueryAsync(AnalyzeRequest request)
    {
        _logger.LogInformation("Analyzing query: {Query}", request.Query);

        await Task.Delay(50); // Simulate processing time

        var response = new AnalyzeResponse
        {
            Intent = DetermineIntent(request.Query),
            Entities = ExtractEntities(request.Query),
            Confidence = new Dictionary<string, float>
            {
                { "intent", 0.92f },
                { "entities", 0.87f }
            },
            Recommendations = GenerateRecommendations(request.Query)
        };

        return response;
    }

    public List<AgentCapability> GetCapabilities()
    {
        var capabilities = new List<AgentCapability>
        {
            new AgentCapability
            {
                Name = "Travel Planning",
                Description = "Assists with creating personalized travel itineraries using Azure AI",
                SupportedLanguages = new List<string> { "en", "es", "fr", "de" },
                IsAvailable = _projectClient != null
            },
            new AgentCapability
            {
                Name = "Destination Recommendations",
                Description = "Provides AI-powered recommendations for travel destinations",
                SupportedLanguages = new List<string> { "en", "es", "fr", "de", "ja" },
                IsAvailable = _projectClient != null
            },
            new AgentCapability
            {
                Name = "Query Understanding",
                Description = "Analyzes and understands user travel queries using Azure AI",
                SupportedLanguages = new List<string> { "en", "es", "fr", "de", "it" },
                IsAvailable = true
            }
        };

        return capabilities;
    }

    private string GenerateMockResponse(string message)
    {
        var lowerMessage = message.ToLower();

        if (lowerMessage.Contains("iceland"))
        {
            return "Iceland is a fantastic destination! I can help you plan a trip that includes the Golden Circle, Blue Lagoon, and stunning waterfalls. Would you like me to create a detailed itinerary?";
        }
        else if (lowerMessage.Contains("morocco"))
        {
            return "Morocco offers an amazing blend of culture, history, and natural beauty. I recommend visiting Marrakech, the Sahara Desert, and the coastal city of Essaouira. Shall we plan your Moroccan adventure?";
        }
        else if (lowerMessage.Contains("japan"))
        {
            return "Japan is a wonderful destination combining tradition and modernity. Consider exploring Tokyo, Kyoto, and Mount Fuji. Would you like recommendations for the best season to visit?";
        }
        else if (lowerMessage.Contains("budget") || lowerMessage.Contains("cost"))
        {
            return "I can help you plan a trip within your budget. What's your approximate budget range and preferred destination? This will help me provide the most suitable recommendations.";
        }
        else if (lowerMessage.Contains("hello") || lowerMessage.Contains("hi"))
        {
            return "Hello! I'm your AI travel assistant powered by Azure AI. I can help you plan trips, find destinations, and create personalized itineraries. Where would you like to go?";
        }
        else
        {
            return "I understand you're interested in travel planning. Could you tell me more about your preferences? For example, your preferred destination, travel dates, budget, and what type of experiences you're looking for?";
        }
    }

    private List<string> GenerateSuggestions(string message)
    {
        var suggestions = new List<string>();
        var lowerMessage = message.ToLower();

        if (lowerMessage.Contains("iceland"))
        {
            suggestions.Add("Show me a 7-day Iceland itinerary");
            suggestions.Add("What's the best time to visit Iceland?");
            suggestions.Add("Budget for Iceland trip");
        }
        else if (lowerMessage.Contains("morocco"))
        {
            suggestions.Add("Create a Morocco travel plan");
            suggestions.Add("Best places in Morocco");
            suggestions.Add("Morocco travel costs");
        }
        else
        {
            suggestions.Add("Show me popular destinations");
            suggestions.Add("Help me plan a trip");
            suggestions.Add("What destinations do you recommend?");
        }

        return suggestions;
    }

    private string DetermineIntent(string query)
    {
        var lowerQuery = query.ToLower();

        if (lowerQuery.Contains("plan") || lowerQuery.Contains("itinerary"))
            return "trip_planning";
        else if (lowerQuery.Contains("recommend") || lowerQuery.Contains("suggest"))
            return "recommendation_request";
        else if (lowerQuery.Contains("cost") || lowerQuery.Contains("budget") || lowerQuery.Contains("price"))
            return "budget_inquiry";
        else if (lowerQuery.Contains("when") || lowerQuery.Contains("best time"))
            return "timing_inquiry";
        else
            return "general_inquiry";
    }

    private List<string> ExtractEntities(string query)
    {
        var entities = new List<string>();
        var destinations = new[] { "iceland", "morocco", "japan", "italy", "switzerland", "france", "spain" };

        foreach (var destination in destinations)
        {
            if (query.ToLower().Contains(destination))
            {
                entities.Add(destination);
            }
        }

        return entities;
    }

    private List<string> GenerateRecommendations(string query)
    {
        var recommendations = new List<string>
        {
            "Consider booking flights 3-4 months in advance for better prices",
            "Check visa requirements for your destination",
            "Look into travel insurance options"
        };

        if (query.ToLower().Contains("budget"))
        {
            recommendations.Add("Use budget-friendly accommodations like hostels or Airbnb");
            recommendations.Add("Consider traveling during off-peak seasons");
        }

        return recommendations;
    }
}
