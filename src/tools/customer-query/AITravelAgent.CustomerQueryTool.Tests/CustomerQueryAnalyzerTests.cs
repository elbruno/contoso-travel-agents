using System.Diagnostics;

namespace AITravelAgent.CustomerQueryTool.Tests
{
    [TestClass]
    public sealed class CustomerQueryAnalyzerTests
    {
        private static readonly string[] ValidEmotions = [ "happy", "sad", "angry", "neutral" ];
        private static readonly string[] ValidIntents = [ "book_flight", "cancel_flight", "change_flight", "inquire", "complaint" ];
        private static readonly string[] ValidRequirements = [ "business", "economy", "first_class" ];
        private static readonly string[] ValidPreferences = [ "window", "aisle", "extra_legroom" ];

        [TestMethod]
        public async Task AnalyzeAsync_WithValidQuery_ShouldReturnAnalysisResult()
        {
            // Arrange
            var analyzer = new CustomerQueryAnalyzer();
            const string customerQuery = "I want to book a business class flight with a window seat";

            // Act
            var result = await analyzer.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(customerQuery, result.CustomerQuery);
        }

        [TestMethod]
        public async Task AnalyzeAsync_ShouldReturnValidEmotion()
        {
            // Arrange
            var analyzer = new CustomerQueryAnalyzer();
            const string customerQuery = "I need help with my flight";

            // Act
            var result = await analyzer.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsNotNull(result.Emotion);
            Assert.IsTrue(ValidEmotions.Contains(result.Emotion), 
                $"Emotion '{result.Emotion}' is not in the valid emotions list");
        }

        [TestMethod]
        public async Task AnalyzeAsync_ShouldReturnValidIntent()
        {
            // Arrange
            var analyzer = new CustomerQueryAnalyzer();
            const string customerQuery = "I need help with my flight";

            // Act
            var result = await analyzer.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsNotNull(result.Intent);
            Assert.IsTrue(ValidIntents.Contains(result.Intent), 
                $"Intent '{result.Intent}' is not in the valid intents list");
        }

        [TestMethod]
        public async Task AnalyzeAsync_ShouldReturnValidRequirements()
        {
            // Arrange
            var analyzer = new CustomerQueryAnalyzer();
            const string customerQuery = "I need help with my flight";

            // Act
            var result = await analyzer.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsNotNull(result.Requirements);
            Assert.IsTrue(ValidRequirements.Contains(result.Requirements), 
                $"Requirements '{result.Requirements}' is not in the valid requirements list");
        }

        [TestMethod]
        public async Task AnalyzeAsync_ShouldReturnValidPreferences()
        {
            // Arrange
            var analyzer = new CustomerQueryAnalyzer();
            const string customerQuery = "I need help with my flight";

            // Act
            var result = await analyzer.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsNotNull(result.Preferences);
            Assert.IsTrue(ValidPreferences.Contains(result.Preferences), 
                $"Preferences '{result.Preferences}' is not in the valid preferences list");
        }

        [TestMethod]
        public async Task AnalyzeAsync_WithEmptyString_ShouldReturnAnalysisResult()
        {
            // Arrange
            var analyzer = new CustomerQueryAnalyzer();
            const string customerQuery = "";

            // Act
            var result = await analyzer.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(customerQuery, result.CustomerQuery);
            Assert.IsNotNull(result.Emotion);
            Assert.IsNotNull(result.Intent);
            Assert.IsNotNull(result.Requirements);
            Assert.IsNotNull(result.Preferences);
        }

        [TestMethod]
        public async Task AnalyzeAsync_WithNullInput_ShouldReturnAnalysisResult()
        {
            // Arrange
            var analyzer = new CustomerQueryAnalyzer();

            // Act
            var result = await analyzer.AnalyzeAsync(null!);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.CustomerQuery);
            Assert.IsNotNull(result.Emotion);
            Assert.IsNotNull(result.Intent);
            Assert.IsNotNull(result.Requirements);
            Assert.IsNotNull(result.Preferences);
        }

        [TestMethod]
        public async Task AnalyzeAsync_ShouldTakeAtLeastOneSecond()
        {
            // Arrange
            var analyzer = new CustomerQueryAnalyzer();
            const string customerQuery = "Test query";
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            await analyzer.AnalyzeAsync(customerQuery);
            stopwatch.Stop();

            // Assert - Allow some timing tolerance due to system variations
            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= 950, 
                $"Method should take approximately 1000ms but took {stopwatch.ElapsedMilliseconds}ms");
        }

        [TestMethod]
        public async Task AnalyzeAsync_MultipleCallsWithSameInput_MayReturnDifferentResults()
        {
            // Arrange
            var analyzer = new CustomerQueryAnalyzer();
            const string customerQuery = "Test query";
            var results = new List<CustomerQueryAnalysisResult>();

            // Act - Make multiple calls
            for (int i = 0; i < 10; i++)
            {
                var result = await analyzer.AnalyzeAsync(customerQuery);
                results.Add(result);
            }

            // Assert - Since it uses Random.Shared, we should get some variation
            // (This test might occasionally fail due to randomness, but very unlikely with 10 calls)
            var uniqueEmotions = results.Select(r => r.Emotion).Distinct().Count();
            var uniqueIntents = results.Select(r => r.Intent).Distinct().Count();
            var uniqueRequirements = results.Select(r => r.Requirements).Distinct().Count();
            var uniquePreferences = results.Select(r => r.Preferences).Distinct().Count();

            // At least one category should have variation (this is probabilistic but very likely)
            var hasVariation = uniqueEmotions > 1 || uniqueIntents > 1 || 
                             uniqueRequirements > 1 || uniquePreferences > 1;
            
            Assert.IsTrue(hasVariation, 
                "Multiple calls should produce some variation in results due to randomness");
            
            // All results should have the same customer query
            Assert.IsTrue(results.All(r => r.CustomerQuery == customerQuery),
                "All results should preserve the original customer query");
        }

        [TestMethod]
        public async Task AnalyzeAsync_ShouldReturnNewInstanceEachTime()
        {
            // Arrange
            var analyzer = new CustomerQueryAnalyzer();
            const string customerQuery = "Test query";

            // Act
            var result1 = await analyzer.AnalyzeAsync(customerQuery);
            var result2 = await analyzer.AnalyzeAsync(customerQuery);

            // Assert
            Assert.AreNotSame(result1, result2, "Each call should return a new instance");
        }
    }
}