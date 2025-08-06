using AITravelAgent.CustomerQueryTool;

namespace AITravelAgent.CustomerQueryTool.Tests
{
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
        public async Task AnalyzeAsync_WithValidQuery_ReturnsAnalysisResult()
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

            // Verify emotion is one of the expected values
            var validEmotions = new[] { "happy", "sad", "angry", "neutral" };
            Assert.IsTrue(validEmotions.Contains(result.Emotion));

            // Verify intent is one of the expected values
            var validIntents = new[] { "book_flight", "cancel_flight", "change_flight", "inquire", "complaint" };
            Assert.IsTrue(validIntents.Contains(result.Intent));

            // Verify requirements is one of the expected values
            var validRequirements = new[] { "business", "economy", "first_class" };
            Assert.IsTrue(validRequirements.Contains(result.Requirements));

            // Verify preferences is one of the expected values
            var validPreferences = new[] { "window", "aisle", "extra_legroom" };
            Assert.IsTrue(validPreferences.Contains(result.Preferences));
        }

        [TestMethod]
        public async Task AnalyzeAsync_WithNullQuery_ReturnsAnalysisResultWithNullQuery()
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
        public async Task AnalyzeAsync_WithEmptyQuery_ReturnsAnalysisResultWithEmptyQuery()
        {
            // Arrange
            var customerQuery = "";

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
        public async Task AnalyzeAsync_MultipleCallsWithSameQuery_MayReturnDifferentResults()
        {
            // Arrange
            var customerQuery = "I need help with my flight reservation";

            // Act
            var result1 = await _analyzer.AnalyzeAsync(customerQuery);
            var result2 = await _analyzer.AnalyzeAsync(customerQuery);

            // Assert
            // Since the analyzer uses random selection, results may differ
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(customerQuery, result1.CustomerQuery);
            Assert.AreEqual(customerQuery, result2.CustomerQuery);

            // Note: Due to randomness, emotion, intent, requirements, and preferences 
            // may be different between calls, which is expected behavior
        }

        [TestMethod]
        public async Task AnalyzeAsync_WithLongQuery_ReturnsValidAnalysisResult()
        {
            // Arrange
            var customerQuery = "I am extremely frustrated because my flight was cancelled last minute and I need to find an alternative business class seat for tomorrow morning to attend an important business meeting in London. I prefer a window seat and need extra legroom due to my height.";

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
        public async Task AnalyzeAsync_HasExpectedDelay()
        {
            // Arrange
            var customerQuery = "Test query";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            await _analyzer.AnalyzeAsync(customerQuery);

            // Assert
            stopwatch.Stop();
            // The analyzer has a 1 second delay, so elapsed time should be at least 1000ms
            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= 1000, 
                $"Expected at least 1000ms delay, but got {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
