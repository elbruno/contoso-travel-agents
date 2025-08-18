using AITravelAgent.CustomerQueryTool;

namespace AITravelAgent.CustomerQueryTool.Tests
{
    [TestClass]
    public sealed class CustomerQueryAnalysisResultTests
    {
        [TestMethod]
        public void CustomerQueryAnalysisResult_DefaultValues_ShouldBeNull()
        {
            // Arrange & Act
            var result = new CustomerQueryAnalysisResult();

            // Assert
            Assert.IsNull(result.CustomerQuery);
            Assert.IsNull(result.Emotion);
            Assert.IsNull(result.Intent);
            Assert.IsNull(result.Requirements);
            Assert.IsNull(result.Preferences);
        }

        [TestMethod]
        public void CustomerQueryAnalysisResult_PropertyAssignment_ShouldWork()
        {
            // Arrange
            var result = new CustomerQueryAnalysisResult();
            var expectedQuery = "Test query";
            var expectedEmotion = "happy";
            var expectedIntent = "book_flight";
            var expectedRequirements = "business";
            var expectedPreferences = "window";

            // Act
            result.CustomerQuery = expectedQuery;
            result.Emotion = expectedEmotion;
            result.Intent = expectedIntent;
            result.Requirements = expectedRequirements;
            result.Preferences = expectedPreferences;

            // Assert
            Assert.AreEqual(expectedQuery, result.CustomerQuery);
            Assert.AreEqual(expectedEmotion, result.Emotion);
            Assert.AreEqual(expectedIntent, result.Intent);
            Assert.AreEqual(expectedRequirements, result.Requirements);
            Assert.AreEqual(expectedPreferences, result.Preferences);
        }

        [TestMethod]
        public void CustomerQueryAnalysisResult_ObjectInitializer_ShouldWork()
        {
            // Arrange
            var expectedQuery = "Test query with initializer";
            var expectedEmotion = "neutral";
            var expectedIntent = "inquire";
            var expectedRequirements = "economy";
            var expectedPreferences = "aisle";

            // Act
            var result = new CustomerQueryAnalysisResult
            {
                CustomerQuery = expectedQuery,
                Emotion = expectedEmotion,
                Intent = expectedIntent,
                Requirements = expectedRequirements,
                Preferences = expectedPreferences
            };

            // Assert
            Assert.AreEqual(expectedQuery, result.CustomerQuery);
            Assert.AreEqual(expectedEmotion, result.Emotion);
            Assert.AreEqual(expectedIntent, result.Intent);
            Assert.AreEqual(expectedRequirements, result.Requirements);
            Assert.AreEqual(expectedPreferences, result.Preferences);
        }

        [TestMethod]
        public void CustomerQueryAnalysisResult_NullValues_ShouldBeAccepted()
        {
            // Arrange & Act
            var result = new CustomerQueryAnalysisResult
            {
                CustomerQuery = null,
                Emotion = null,
                Intent = null,
                Requirements = null,
                Preferences = null
            };

            // Assert
            Assert.IsNull(result.CustomerQuery);
            Assert.IsNull(result.Emotion);
            Assert.IsNull(result.Intent);
            Assert.IsNull(result.Requirements);
            Assert.IsNull(result.Preferences);
        }
    }
}