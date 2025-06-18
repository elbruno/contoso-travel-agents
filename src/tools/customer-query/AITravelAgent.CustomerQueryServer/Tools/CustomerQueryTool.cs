using System.ComponentModel;
using ModelContextProtocol.Server;
using AITravelAgent.CustomerQueryTool; // Import the external analyzer

namespace AITravelAgent.CustomerQueryServer.Tools;

[McpServerToolType]
public class CustomerQueryTool(ILogger<CustomerQueryTool> logger)
{
    private readonly CustomerQueryAnalyzer _analyzer = new();

    [McpServerTool(Name = "analyze_customer_query", Title = "Analyze Customer Query")]
    [Description("Analyzes the customer query and provides a response.")]
    public async Task<CustomerQueryAnalysisResult> AnalyzeCustomerQueryAsync(
        [Description("The customer query to analyze")] string customerQuery)
    {
        logger.LogInformation("Received customer query: {customerQuery}", customerQuery);
        // Delegate to the external analyzer and map the result
        var result = await _analyzer.AnalyzeAsync(customerQuery);
        return new CustomerQueryAnalysisResult
        {
            CustomerQuery = result.CustomerQuery,
            Emotion = result.Emotion,
            Intent = result.Intent,
            Requirements = result.Requirements,
            Preferences = result.Preferences
        };
    }
}
