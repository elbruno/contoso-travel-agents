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
            string testQuery = "I want to book a business class flight to Paris";

            // Act
            var result = await _analyzer.AnalyzeAsync(testQuery);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(testQuery, result.CustomerQuery);
            Assert.IsNotNull(result.Emotion);
            Assert.IsNotNull(result.Intent);
            Assert.IsNotNull(result.Requirements);
            Assert.IsNotNull(result.Preferences);
        }

        [TestMethod]
        public async Task AnalyzeAsync_WithEmptyQuery_ReturnsAnalysisResult()
        {
            // Arrange
            string testQuery = "";

            // Act
            var result = await _analyzer.AnalyzeAsync(testQuery);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(testQuery, result.CustomerQuery);
            Assert.IsNotNull(result.Emotion);
            Assert.IsNotNull(result.Intent);
            Assert.IsNotNull(result.Requirements);
            Assert.IsNotNull(result.Preferences);
        }

        [TestMethod]
        public async Task AnalyzeAsync_EmotionField_ContainsValidValues()
        {
            // Arrange
            string testQuery = "Test query";
            var validEmotions = new[] { "happy", "sad", "angry", "neutral" };

            // Act
            var result = await _analyzer.AnalyzeAsync(testQuery);

            // Assert
            Assert.IsTrue(validEmotions.Contains(result.Emotion));
        }

        [TestMethod]
        public async Task AnalyzeAsync_IntentField_ContainsValidValues()
        {
            // Arrange
            string testQuery = "Test query";
            var validIntents = new[] { "book_flight", "cancel_flight", "change_flight", "inquire", "complaint" };

            // Act
            var result = await _analyzer.AnalyzeAsync(testQuery);

            // Assert
            Assert.IsTrue(validIntents.Contains(result.Intent));
        }

        [TestMethod]
        public async Task AnalyzeAsync_RequirementsField_ContainsValidValues()
        {
            // Arrange
            string testQuery = "Test query";
            var validRequirements = new[] { "business", "economy", "first_class" };

            // Act
            var result = await _analyzer.AnalyzeAsync(testQuery);

            // Assert
            Assert.IsTrue(validRequirements.Contains(result.Requirements));
        }

        [TestMethod]
        public async Task AnalyzeAsync_PreferencesField_ContainsValidValues()
        {
            // Arrange
            string testQuery = "Test query";
            var validPreferences = new[] { "window", "aisle", "extra_legroom" };

            // Act
            var result = await _analyzer.AnalyzeAsync(testQuery);

            // Assert
            Assert.IsTrue(validPreferences.Contains(result.Preferences));
        }

        [TestMethod]
        public async Task AnalyzeAsync_WithNullQuery_HandlesGracefully()
        {
            // Arrange
            string testQuery = null!;

            // Act
            var result = await _analyzer.AnalyzeAsync(testQuery);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(testQuery, result.CustomerQuery);
        }

        [TestMethod]
        public async Task AnalyzeAsync_ExecutionTime_ShouldBeReasonable()
        {
            // Arrange
            string testQuery = "Test query for timing";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var result = await _analyzer.AnalyzeAsync(testQuery);
            stopwatch.Stop();

            // Assert
            Assert.IsNotNull(result);
            // Should complete within 3 seconds (accounting for the 1-second delay in implementation)
            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= 900); // Allow some variance
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 3000); // More generous upper bound
        }

        [TestMethod]
        public async Task AnalyzeAsync_MultipleExecutions_ProduceVariedResults()
        {
            // Arrange
            string testQuery = "Test query for randomness";
            var results = new List<CustomerQueryAnalysisResult>();

            // Act - run multiple times to check for randomness
            for (int i = 0; i < 10; i++)
            {
                var result = await _analyzer.AnalyzeAsync(testQuery);
                results.Add(result);
            }

            // Assert - should have some variation in at least one field across multiple runs
            var uniqueEmotions = results.Select(r => r.Emotion).Distinct().Count();
            var uniqueIntents = results.Select(r => r.Intent).Distinct().Count();
            var uniqueRequirements = results.Select(r => r.Requirements).Distinct().Count();
            var uniquePreferences = results.Select(r => r.Preferences).Distinct().Count();

            // At least one field should show variation (not all results identical)
            Assert.IsTrue(uniqueEmotions > 1 || uniqueIntents > 1 || uniqueRequirements > 1 || uniquePreferences > 1,
                "Expected some variation in analysis results across multiple executions");
        }
    }
}
