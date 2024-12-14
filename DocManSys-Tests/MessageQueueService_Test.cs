using System.Text;
using DocManSys_RestAPI.Services;
using FakeItEasy;
using RabbitMQ.Client;

namespace DocManSys_Tests;

public class MessageQueueServiceTests {
    private IConnection _mockConnection;
    private IModel _mockChannel;
    private MessageQueueService _service;

    [SetUp]
    public void Setup() {
        // Create mocks for the RabbitMQ connection and channel
        _mockConnection = A.Fake<IConnection>();
        _mockChannel = A.Fake<IModel>();

        // Set up the fake connection to return the fake channel
        A.CallTo(() => _mockConnection.CreateModel()).Returns(_mockChannel);

        // Initialize the service with the mocked connection
        A.CallTo(() => _mockConnection.IsOpen).Returns(true);
        A.CallTo(() => _mockChannel.IsOpen).Returns(true);
        _service = new MessageQueueService(_mockConnection);
    }

    [Test]
    public void Dispose_ShouldCloseChannelAndConnection_WhenDisposed() {
        // Act
        _service.Dispose();

        // Assert
        A.CallTo(() => _mockChannel.Close()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mockConnection.Close()).MustHaveHappenedOnceExactly();
    }

    [TearDown]
    public void TearDown() {
        _mockConnection.Dispose();
        _mockChannel.Dispose();
        _service.Dispose();
    }
}