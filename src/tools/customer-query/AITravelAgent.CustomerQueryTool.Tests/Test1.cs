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
            var customerQuery = "I want to book a flight to Paris";

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
        public async Task AnalyzeAsync_WithEmptyQuery_ReturnsAnalysisResult()
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
        public async Task AnalyzeAsync_ReturnsValidEmotions()
        {
            // Arrange
            var customerQuery = "I am frustrated with my flight booking";
            var validEmotions = new[] { "happy", "sad", "angry", "neutral" };

            // Act
            var result = await _analyzer.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsTrue(validEmotions.Contains(result.Emotion), 
                $"Emotion '{result.Emotion}' is not in the valid emotions list");
        }

        [TestMethod]
        public async Task AnalyzeAsync_ReturnsValidIntents()
        {
            // Arrange
            var customerQuery = "I need to cancel my booking";
            var validIntents = new[] { "book_flight", "cancel_flight", "change_flight", "inquire", "complaint" };

            // Act
            var result = await _analyzer.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsTrue(validIntents.Contains(result.Intent), 
                $"Intent '{result.Intent}' is not in the valid intents list");
        }

        [TestMethod]
        public async Task AnalyzeAsync_ReturnsValidRequirements()
        {
            // Arrange
            var customerQuery = "I prefer business class for my trip";
            var validRequirements = new[] { "business", "economy", "first_class" };

            // Act
            var result = await _analyzer.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsTrue(validRequirements.Contains(result.Requirements), 
                $"Requirements '{result.Requirements}' is not in the valid requirements list");
        }

        [TestMethod]
        public async Task AnalyzeAsync_ReturnsValidPreferences()
        {
            // Arrange
            var customerQuery = "I would like a window seat";
            var validPreferences = new[] { "window", "aisle", "extra_legroom" };

            // Act
            var result = await _analyzer.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsTrue(validPreferences.Contains(result.Preferences), 
                $"Preferences '{result.Preferences}' is not in the valid preferences list");
        }

        [TestMethod]
        public async Task AnalyzeAsync_WithNullQuery_ReturnsAnalysisResult()
        {
            // Arrange
            string? customerQuery = null;

            // Act
            var result = await _analyzer.AnalyzeAsync(customerQuery!);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(customerQuery, result.CustomerQuery);
        }

        [TestMethod]
        public async Task AnalyzeAsync_HasConsistentDelay()
        {
            // Arrange
            var customerQuery = "Test query for timing";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            await _analyzer.AnalyzeAsync(customerQuery);
            stopwatch.Stop();

            // Assert
            // Should take approximately 1 second (with some tolerance for execution time)
            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= 900, 
                "Analysis should take at least 900ms");
            Assert.IsTrue(stopwatch.ElapsedMilliseconds <= 1200, 
                "Analysis should not take more than 1200ms");
        }
    }
}
