using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using DocManSys_OCR_Worker;

namespace DocManSys_Tests
{
    [TestFixture]
    public class OcrWorker_Test
    {
        //public required IConnection _fakeConnection;
        //public required IModel _fakeChannel;
        //public required OcrWorker _ocrWorker;

        //[SetUp]
        //public void Setup()
        //{
        //    _fakeConnection = A.Fake<IConnection>();
        //    _fakeChannel = A.Fake<IModel>();

        //    A.CallTo(() => _fakeConnection.CreateModel()).Returns(_fakeChannel);

        //    _ocrWorker = new OcrWorker();
        //}

        //[Test]
        //public void ConnectToRabbitMq_ShouldThrowException_WhenConnectionFails()
        //{
        //    // Arrange
        //    A.CallTo(() => _fakeConnection.IsOpen).Returns(false);

        //    // Act & Assert
        //    Assert.Throws<Exception>(() => _ocrWorker.ConnectToRabbitMq());
        //}

        //[Test]
        //public void PerformOcr_ShouldReturnEmptyString_WhenFileDoesNotExist()
        //{
        //    // Arrange
        //    var filePath = "non_existent_file.png";

        //    // Act
        //    var result = _ocrWorker.PerformOcr(filePath);

        //    // Assert
        //    Assert.IsEmpty(result);
        //}

        //[Test]
        //public void Start_ShouldConsumeMessages()
        //{
        //    // Arrange
        //    var consumer = new EventingBasicConsumer(_fakeChannel);
        //    var body = Encoding.UTF8.GetBytes("1|test_image.png");

        //    A.CallTo(() => _fakeChannel.BasicConsume(A<string>.Ignored, A<bool>.Ignored, A<IBasicConsumer>.Ignored))
        //        .Invokes((string queue, bool autoAck, IBasicConsumer consumer) =>
        //        {
        //            consumer.HandleBasicDeliver("consumer_tag", 1, false, "exchange", "routing_key", null, body);
        //        });

        //    // Act
        //    _ocrWorker.Start();

        //    // Assert
        //    A.CallTo(() => _fakeChannel.BasicConsume(A<string>.Ignored, A<bool>.Ignored, A<IBasicConsumer>.Ignored)).MustHaveHappenedOnceExactly();
        //}
        //[TearDown]
        //public void TearDown()
        //{
        //    _fakeConnection.Close();
        //    _fakeChannel.Close();
        //    _ocrWorker.Dispose();

        //}
    }
}

