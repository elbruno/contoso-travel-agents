namespace AITravelAgent.CustomerQueryTool;

public class CustomerQueryAnalyzer
{
    private static readonly string[] emotions = [ "happy", "sad", "angry", "neutral" ];
    private static readonly string[] intents = [ "book_flight", "cancel_flight", "change_flight", "inquire", "complaint" ];
    private static readonly string[] requirements = [ "business", "economy", "first_class" ];
    private static readonly string[] preferences = [ "window", "aisle", "extra_legroom" ];
    private static readonly Random random = Random.Shared;

    public async Task<CustomerQueryAnalysisResult> AnalyzeAsync(string customerQuery)
    {
        await Task.Delay(1000);
        return new CustomerQueryAnalysisResult
        {
            CustomerQuery = customerQuery,
            Emotion = emotions[random.Next(emotions.Length)],
            Intent = intents[random.Next(intents.Length)],
            Requirements = requirements[random.Next(requirements.Length)],
            Preferences = preferences[random.Next(preferences.Length)]
        };
    }
}
