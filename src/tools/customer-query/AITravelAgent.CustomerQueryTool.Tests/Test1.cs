using AITravelAgent.CustomerQueryTool;

namespace AITravelAgent.CustomerQueryTool.Tests
{
    [TestClass]
    public sealed class CustomerQueryAnalyzerTests
    {
        private CustomerQueryAnalyzer? _analyzer;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            // This method is called once for the test class, before any tests of the class are run.
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // This method is called once for the test class, after all tests of the class are run.
        }

        [TestInitialize]
        public void TestInit()
        {
            // This method is called before each test method.
            _analyzer = new CustomerQueryAnalyzer();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // This method is called after each test method.
            _analyzer = null;
        }

        [TestMethod]
        public async Task AnalyzeAsync_WithValidQuery_ReturnsValidResult()
        {
            // Arrange
            var customerQuery = "I want to book a business class flight to New York with a window seat";

            // Act
            var result = await _analyzer!.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(customerQuery, result.CustomerQuery);
            Assert.IsNotNull(result.Emotion);
            Assert.IsNotNull(result.Intent);
            Assert.IsNotNull(result.Requirements);
            Assert.IsNotNull(result.Preferences);
        }

        [TestMethod]
        public async Task AnalyzeAsync_WithEmptyQuery_ReturnsValidResult()
        {
            // Arrange
            var customerQuery = "";

            // Act
            var result = await _analyzer!.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(customerQuery, result.CustomerQuery);
            Assert.IsNotNull(result.Emotion);
            Assert.IsNotNull(result.Intent);
            Assert.IsNotNull(result.Requirements);
            Assert.IsNotNull(result.Preferences);
        }

        [TestMethod]
        public async Task AnalyzeAsync_WithNullQuery_HandlesGracefully()
        {
            // Arrange
            string? customerQuery = null;

            // Act
            var result = await _analyzer!.AnalyzeAsync(customerQuery!);

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
            var customerQuery = "Test query";
            var validEmotions = new[] { "happy", "sad", "angry", "neutral" };

            // Act
            var result = await _analyzer!.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsTrue(validEmotions.Contains(result.Emotion));
        }

        [TestMethod]
        public async Task AnalyzeAsync_ReturnsValidIntents()
        {
            // Arrange
            var customerQuery = "Test query";
            var validIntents = new[] { "book_flight", "cancel_flight", "change_flight", "inquire", "complaint" };

            // Act
            var result = await _analyzer!.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsTrue(validIntents.Contains(result.Intent));
        }

        [TestMethod]
        public async Task AnalyzeAsync_ReturnsValidRequirements()
        {
            // Arrange
            var customerQuery = "Test query";
            var validRequirements = new[] { "business", "economy", "first_class" };

            // Act
            var result = await _analyzer!.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsTrue(validRequirements.Contains(result.Requirements));
        }

        [TestMethod]
        public async Task AnalyzeAsync_ReturnsValidPreferences()
        {
            // Arrange
            var customerQuery = "Test query";
            var validPreferences = new[] { "window", "aisle", "extra_legroom" };

            // Act
            var result = await _analyzer!.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsTrue(validPreferences.Contains(result.Preferences));
        }

        [TestMethod]
        public async Task AnalyzeAsync_MultipleCallsWithSameInput_MayReturnDifferentResults()
        {
            // Arrange
            var customerQuery = "Test query for randomness";
            var results = new List<CustomerQueryAnalysisResult>();

            // Act - Call multiple times
            for (int i = 0; i < 10; i++)
            {
                var result = await _analyzer!.AnalyzeAsync(customerQuery);
                results.Add(result);
            }

            // Assert - Due to randomness, we might get different results
            // Check that we have at least one result and all are valid
            Assert.AreEqual(10, results.Count);
            Assert.IsTrue(results.All(r => r.CustomerQuery == customerQuery));
            Assert.IsTrue(results.All(r => !string.IsNullOrEmpty(r.Emotion)));
            Assert.IsTrue(results.All(r => !string.IsNullOrEmpty(r.Intent)));
            Assert.IsTrue(results.All(r => !string.IsNullOrEmpty(r.Requirements)));
            Assert.IsTrue(results.All(r => !string.IsNullOrEmpty(r.Preferences)));
        }

        [TestMethod]
        public async Task AnalyzeAsync_TakesApproximatelyOneSecond()
        {
            // Arrange
            var customerQuery = "Performance test query";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            await _analyzer!.AnalyzeAsync(customerQuery);

            // Assert
            stopwatch.Stop();
            // Should take approximately 1 second, allow some tolerance for test execution
            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= 900);
            Assert.IsTrue(stopwatch.ElapsedMilliseconds <= 1200);
        }

        [TestMethod]
        public async Task AnalyzeAsync_WithComplexQuery_ReturnsValidResult()
        {
            // Arrange
            var customerQuery = "I'm really frustrated! I need to cancel my first-class flight to London and book a new economy flight to Paris instead. I prefer an aisle seat and have special dietary requirements.";

            // Act
            var result = await _analyzer!.AnalyzeAsync(customerQuery);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(customerQuery, result.CustomerQuery);
            Assert.IsNotNull(result.Emotion);
            Assert.IsNotNull(result.Intent);
            Assert.IsNotNull(result.Requirements);
            Assert.IsNotNull(result.Preferences);
        }
    }
}
