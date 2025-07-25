using AITravelAgent.CustomerQueryTool;

namespace AITravelAgent.CustomerQueryTool.Tests;

[TestClass]
public sealed class CustomerQueryAnalyzerTests
{
    private CustomerQueryAnalyzer _analyzer = null!;

    [TestInitialize]
    public void TestInit()
    {
        _analyzer = new CustomerQueryAnalyzer();
    }

    [TestMethod]
    public async Task AnalyzeAsync_ShouldReturnValidResult_WhenGivenCustomerQuery()
    {
        // Arrange
        var customerQuery = "I want to book a business class flight to Paris";

        // Act
        var result = await _analyzer.AnalyzeAsync(customerQuery);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(customerQuery, result.CustomerQuery);
        Assert.IsNotNull(result.Emotion);
        Assert.IsNotNull(result.Intent);
        Assert.IsNotNull(result.Requirements);
        Assert.IsNotNull(result.Preferences);
    }

    [TestMethod]
    public async Task AnalyzeAsync_ShouldReturnValidEmotion_WhenGivenCustomerQuery()
    {
        // Arrange
        var customerQuery = "I'm very happy with your service!";
        var validEmotions = new[] { "happy", "sad", "angry", "neutral" };

        // Act
        var result = await _analyzer.AnalyzeAsync(customerQuery);

        // Assert
        Assert.IsTrue(validEmotions.Contains(result.Emotion), 
            $"Expected emotion to be one of [{string.Join(", ", validEmotions)}], but got '{result.Emotion}'");
    }

    [TestMethod]
    public async Task AnalyzeAsync_ShouldReturnValidIntent_WhenGivenCustomerQuery()
    {
        // Arrange
        var customerQuery = "I need to cancel my flight reservation";
        var validIntents = new[] { "book_flight", "cancel_flight", "change_flight", "inquire", "complaint" };

        // Act
        var result = await _analyzer.AnalyzeAsync(customerQuery);

        // Assert
        Assert.IsTrue(validIntents.Contains(result.Intent),
            $"Expected intent to be one of [{string.Join(", ", validIntents)}], but got '{result.Intent}'");
    }

    [TestMethod]
    public async Task AnalyzeAsync_ShouldReturnValidRequirements_WhenGivenCustomerQuery()
    {
        // Arrange
        var customerQuery = "I need first class tickets";
        var validRequirements = new[] { "business", "economy", "first_class" };

        // Act
        var result = await _analyzer.AnalyzeAsync(customerQuery);

        // Assert
        Assert.IsTrue(validRequirements.Contains(result.Requirements),
            $"Expected requirements to be one of [{string.Join(", ", validRequirements)}], but got '{result.Requirements}'");
    }

    [TestMethod]
    public async Task AnalyzeAsync_ShouldReturnValidPreferences_WhenGivenCustomerQuery()
    {
        // Arrange
        var customerQuery = "I prefer a window seat";
        var validPreferences = new[] { "window", "aisle", "extra_legroom" };

        // Act
        var result = await _analyzer.AnalyzeAsync(customerQuery);

        // Assert
        Assert.IsTrue(validPreferences.Contains(result.Preferences),
            $"Expected preferences to be one of [{string.Join(", ", validPreferences)}], but got '{result.Preferences}'");
    }

    [TestMethod]
    public async Task AnalyzeAsync_ShouldHandleEmptyString_Gracefully()
    {
        // Arrange
        var customerQuery = string.Empty;

        // Act
        var result = await _analyzer.AnalyzeAsync(customerQuery);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(customerQuery, result.CustomerQuery);
        Assert.IsNotNull(result.Emotion);
        Assert.IsNotNull(result.Intent);
        Assert.IsNotNull(result.Requirements);
        Assert.IsNotNull(result.Preferences);
    }

    [TestMethod]
    public async Task AnalyzeAsync_ShouldHandleNullInput_Gracefully()
    {
        // Arrange
        string? customerQuery = null;

        // Act
        var result = await _analyzer.AnalyzeAsync(customerQuery!);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(customerQuery, result.CustomerQuery);
        Assert.IsNotNull(result.Emotion);
        Assert.IsNotNull(result.Intent);
        Assert.IsNotNull(result.Requirements);
        Assert.IsNotNull(result.Preferences);
    }

    [TestMethod]
    public async Task AnalyzeAsync_ShouldTakeApproximatelyOneSecond()
    {
        // Arrange
        var customerQuery = "Test query for timing";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        await _analyzer.AnalyzeAsync(customerQuery);

        // Assert
        stopwatch.Stop();
        Assert.IsTrue(stopwatch.ElapsedMilliseconds >= 950, 
            $"Expected at least 950ms delay, but got {stopwatch.ElapsedMilliseconds}ms");
        Assert.IsTrue(stopwatch.ElapsedMilliseconds <= 1100, 
            $"Expected at most 1100ms delay, but got {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task AnalyzeAsync_ShouldReturnConsistentStructure_ForMultipleCalls()
    {
        // Arrange
        var customerQuery = "I want to book a flight";

        // Act
        var result1 = await _analyzer.AnalyzeAsync(customerQuery);
        var result2 = await _analyzer.AnalyzeAsync(customerQuery);

        // Assert
        Assert.AreEqual(result1.CustomerQuery, result2.CustomerQuery);
        // Note: Results may vary due to randomness, but structure should be consistent
        Assert.IsNotNull(result1.Emotion);
        Assert.IsNotNull(result1.Intent);
        Assert.IsNotNull(result1.Requirements);
        Assert.IsNotNull(result1.Preferences);
        Assert.IsNotNull(result2.Emotion);
        Assert.IsNotNull(result2.Intent);
        Assert.IsNotNull(result2.Requirements);
        Assert.IsNotNull(result2.Preferences);
    }
}
