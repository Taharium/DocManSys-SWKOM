//using System.IO;
//using System.Threading.Tasks;
//using FakeItEasy;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Minio;
//using Minio.DataModel.Args;
//using NUnit.Framework;
//using DocManSys_RestAPI.Services;
//using System.Text;
//using Minio.DataModel;
//using NUnit.Framework.Internal;

//namespace DocManSys_Tests
//{
//    [TestFixture]
//    public class MinioClientService_Test
//    {
//        private IMinioClient _mockMinioClient;
//        private MinioClientService _service;

//        [SetUp]
//        public void SetUp()
//        {
//            _mockMinioClient = A.Fake<IMinioClient>();
//            _service = new MinioClientService(_mockMinioClient);
//        }

//        [Test]
//        public async Task UploadFile_ShouldCallPutObjectAsync()
//        {
//            // Arrange
//            var mockFile = A.Fake<IFormFile>();
//            var fileStream = new MemoryStream();
//            A.CallTo(() => mockFile.OpenReadStream()).Returns(fileStream);
//            A.CallTo(() => mockFile.FileName).Returns("test-file.txt");
//            A.CallTo(() => mockFile.Length).Returns(fileStream.Length);

//            // Act
//            await _service.UploadFile(mockFile);


//            // Assert

//        }



//        [Test]
//        public async Task DownloadFile_ShouldReturnFileStreamResult()
//        {
//            // Arrange
//            var fileName = "test-file.txt";
//            var fileContent = new MemoryStream();
//            fileContent.Write(new byte[] { 1, 2, 3, 4 });
//            fileContent.Position = 0;

//            A.CallTo(() => _mockMinioClient.GetObjectAsync(A<GetObjectArgs>.That.Matches(args => args.BucketName == "test-bucket" && args.ObjectName == fileName), A<Stream>.That.IsNotNull())).Invokes(call =>
//            {
//                var stream = call.GetArgument<Stream>(1);
//                fileContent.CopyTo(stream);
//            });

//            // Act
//            var result = await _service.DownloadFile(fileName) as FileStreamResult;

//            // Assert
//            Assert.IsNotNull(result);
//            Assert.That(result.ContentType, Is.EqualTo("application/octet-stream"));
//            Assert.That(result.FileDownloadName, Is.EqualTo(fileName));
//        }

//        [Test]
//        public async Task DeleteFile_ShouldCallRemoveObjectAsync()
//        {
//            // Arrange
//            var fileName = "test-file.txt";

//            // Act
//            var result = await _service.DeleteFile(fileName) as OkObjectResult;

//            // Assert
//            Assert.IsNotNull(result);
//            Assert.That(result.StatusCode, Is.EqualTo(200));
//            Assert.That(result.Value, Is.EqualTo($"Datei '{fileName}' erfolgreich gelöscht."));

//            A.CallTo(() => _mockMinioClient.RemoveObjectAsync(A<RemoveObjectArgs>.That.Matches(args => args.BucketName == "test-bucket" && args.ObjectName == fileName))).MustHaveHappened();
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            _mockMinioClient.Dispose();
//        }
//    }
//}

