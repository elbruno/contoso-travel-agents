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
        public void CustomerQueryAnalysisResult_SetProperties_ShouldStoreValues()
        {
            // Arrange
            var result = new CustomerQueryAnalysisResult();
            const string expectedQuery = "I want to book a flight";
            const string expectedEmotion = "happy";
            const string expectedIntent = "book_flight";
            const string expectedRequirements = "business";
            const string expectedPreferences = "window";

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
        public void CustomerQueryAnalysisResult_SetPropertiesToNull_ShouldAcceptNullValues()
        {
            // Arrange
            var result = new CustomerQueryAnalysisResult
            {
                CustomerQuery = "test",
                Emotion = "happy",
                Intent = "book_flight",
                Requirements = "business",
                Preferences = "window"
            };

            // Act
            result.CustomerQuery = null;
            result.Emotion = null;
            result.Intent = null;
            result.Requirements = null;
            result.Preferences = null;

            // Assert
            Assert.IsNull(result.CustomerQuery);
            Assert.IsNull(result.Emotion);
            Assert.IsNull(result.Intent);
            Assert.IsNull(result.Requirements);
            Assert.IsNull(result.Preferences);
        }

        [TestMethod]
        public void CustomerQueryAnalysisResult_SetPropertiesToEmptyString_ShouldStoreEmptyValues()
        {
            // Arrange
            var result = new CustomerQueryAnalysisResult();

            // Act
            result.CustomerQuery = string.Empty;
            result.Emotion = string.Empty;
            result.Intent = string.Empty;
            result.Requirements = string.Empty;
            result.Preferences = string.Empty;

            // Assert
            Assert.AreEqual(string.Empty, result.CustomerQuery);
            Assert.AreEqual(string.Empty, result.Emotion);
            Assert.AreEqual(string.Empty, result.Intent);
            Assert.AreEqual(string.Empty, result.Requirements);
            Assert.AreEqual(string.Empty, result.Preferences);
        }
    }
}
