using System.Collections.Concurrent;
using ContosoTravel.ChatAgentService.Models;

namespace ContosoTravel.ChatAgentService.Services;

public interface IChatAgentService
{
    Task<ChatResponse> ProcessChatAsync(ChatRequest request);
    Task<AnalyzeResponse> AnalyzeQueryAsync(AnalyzeRequest request);
    List<AgentCapability> GetCapabilities();
}

public class ChatAgentService : IChatAgentService
{
    private readonly ILogger<ChatAgentService> _logger;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _sessionHistory = new();

    public ChatAgentService(ILogger<ChatAgentService> logger)
    {
        _logger = logger;
    }

    public async Task<ChatResponse> ProcessChatAsync(ChatRequest request)
    {
        _logger.LogInformation("Processing chat request: {Message}", request.Message);

        var sessionId = request.SessionId ?? Guid.NewGuid().ToString();
        
        // Store conversation history (thread-safe)
        var queue = _sessionHistory.GetOrAdd(sessionId, _ => new ConcurrentQueue<string>());
        queue.Enqueue(request.Message);

        // Simulate AI processing
        await Task.Delay(100); // Simulate processing time

        var response = new ChatResponse
        {
            SessionId = sessionId,
            AgentType = "TravelAssistant",
            Message = GenerateResponse(request.Message),
            Suggestions = GenerateSuggestions(request.Message),
            Timestamp = DateTime.UtcNow
        };

        return response;
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
        return new List<AgentCapability>
        {
            new AgentCapability
            {
                Name = "Travel Planning",
                Description = "Assists with creating personalized travel itineraries",
                SupportedLanguages = new List<string> { "en", "es", "fr", "de" },
                IsAvailable = true
            },
            new AgentCapability
            {
                Name = "Destination Recommendations",
                Description = "Provides recommendations for travel destinations based on preferences",
                SupportedLanguages = new List<string> { "en", "es", "fr", "de", "ja" },
                IsAvailable = true
            },
            new AgentCapability
            {
                Name = "Query Understanding",
                Description = "Analyzes and understands user travel queries",
                SupportedLanguages = new List<string> { "en", "es", "fr", "de", "it" },
                IsAvailable = true
            }
        };
    }

    private string GenerateResponse(string message)
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
            return "Hello! I'm your AI travel assistant. I can help you plan trips, find destinations, and create personalized itineraries. Where would you like to go?";
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
            suggestions.Add("What's the weather like?");
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
