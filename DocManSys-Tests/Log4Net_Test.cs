using System.Collections.Concurrent;
using DocManSys_RestAPI.Log4Net;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace DocManSys_Tests;

[TestFixture]
public class Log4NetProviderTests
{
    private Log4NetProvider _log4NetProvider;
    private ILogger _mockLogger;
    private string _configFilePath = "log4net.config";

    [SetUp]
    public void SetUp()
    {
        // Arrange: Set up the Log4NetProvider and the mock Logger
        _mockLogger = A.Fake<ILogger>();
        _log4NetProvider = new Log4NetProvider(_configFilePath);
    }

    [Test]
    public void CreateLogger_ShouldReturnILogger_WhenCalledWithCategoryName()
    {
        // Arrange
        var categoryName = "TestCategory";

        // Act
        var logger = _log4NetProvider.CreateLogger(categoryName);

        // Assert
        Assert.IsNotNull(logger); // Ensure the logger is not null
        Assert.IsInstanceOf<ILogger>(logger); // Ensure the logger is of type ILogger
    }

    [Test]
    public void CreateLogger_ShouldCreateNewLogger_WhenCalledWithNewCategoryName()
    {
        // Arrange
        var categoryName1 = "Category1";
        var categoryName2 = "Category2";

        // Act
        var logger1 = _log4NetProvider.CreateLogger(categoryName1);
        var logger2 = _log4NetProvider.CreateLogger(categoryName2);

        // Assert
        Assert.AreNotSame(logger1, logger2); // Ensure different loggers for different category names
    }

    [Test]
    public void CreateLogger_ShouldReuseLogger_ForSameCategoryName()
    {
        // Arrange
        var categoryName = "TestCategory";

        // Act
        var logger1 = _log4NetProvider.CreateLogger(categoryName);
        var logger2 = _log4NetProvider.CreateLogger(categoryName);

        // Assert
        Assert.AreSame(logger1, logger2); // Ensure the same logger is reused for the same category
    }

    [Test]
    public void Dispose_ShouldClearLoggersDictionary()
    {
        // Arrange
        var categoryName = "TestCategory";
        _log4NetProvider.CreateLogger(categoryName); // Add a logger to the dictionary

        // Act
        _log4NetProvider.Dispose();

        // Assert
        var loggersField = typeof(Log4NetProvider).GetField("_loggers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var loggers = loggersField.GetValue(_log4NetProvider) as ConcurrentDictionary<string, ILogger>;
        Assert.IsEmpty(loggers); // Ensure the loggers dictionary is cleared
    }

    [Test]
    public void Constructor_ShouldInitializeWithDefaultConfigFile_WhenNoConfigFilePathProvided()
    {
        // Arrange
        var provider = new Log4NetProvider();

        // Act
        var logger = provider.CreateLogger("TestCategory");

        // Assert
        Assert.IsNotNull(logger);
        // Validate the file path used for the logger initialization.
        // You can mock the behavior to check if a FileInfo constructor is invoked with the expected path
    }

    [Test]
    public void Constructor_ShouldInitializeWithProvidedConfigFilePath_WhenConfigFilePathProvided()
    {
        // Arrange
        var customConfigFilePath = "custom_log4net.config";
        var provider = new Log4NetProvider(customConfigFilePath);

        // Act
        var logger = provider.CreateLogger("TestCategory");

        // Assert
        Assert.IsNotNull(logger);
        // Validate the file path used for the logger initialization
        // Similar to the previous test, you'd need to check if the FileInfo constructor was called with the customConfigFilePath
    }

    [TearDown]
    public void TearDown() {
        _log4NetProvider.Dispose();
    }
}