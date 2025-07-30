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
            var customerQuery = "I'm very upset about my delayed flight booking!";

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
        public async Task AnalyzeAsync_WithNullQuery_ReturnsAnalysisResult()
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
        public async Task AnalyzeAsync_ReturnsValidEmotionValues()
        {
            // Arrange
            var customerQuery = "Test query";
            var validEmotions = new[] { "happy", "sad", "angry", "neutral" };

            // Act
            var result = await _analyzer.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsTrue(validEmotions.Contains(result.Emotion));
        }

        [TestMethod]
        public async Task AnalyzeAsync_ReturnsValidIntentValues()
        {
            // Arrange
            var customerQuery = "Test query";
            var validIntents = new[] { "book_flight", "cancel_flight", "change_flight", "inquire", "complaint" };

            // Act
            var result = await _analyzer.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsTrue(validIntents.Contains(result.Intent));
        }

        [TestMethod]
        public async Task AnalyzeAsync_ReturnsValidRequirementValues()
        {
            // Arrange
            var customerQuery = "Test query";
            var validRequirements = new[] { "business", "economy", "first_class" };

            // Act
            var result = await _analyzer.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsTrue(validRequirements.Contains(result.Requirements));
        }

        [TestMethod]
        public async Task AnalyzeAsync_ReturnsValidPreferenceValues()
        {
            // Arrange
            var customerQuery = "Test query";
            var validPreferences = new[] { "window", "aisle", "extra_legroom" };

            // Act
            var result = await _analyzer.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsTrue(validPreferences.Contains(result.Preferences));
        }

        [TestMethod]
        public async Task AnalyzeAsync_TakesAppropriateTime()
        {
            // Arrange
            var customerQuery = "Test query";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            await _analyzer.AnalyzeAsync(customerQuery);

            // Assert
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= 950, "Analysis should take at least 950ms");
            Assert.IsTrue(stopwatch.ElapsedMilliseconds <= 1200, "Analysis should not take more than 1200ms");
        }
    }
}
