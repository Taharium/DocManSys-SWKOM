using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using NUnit.Framework;
using DocManSys_RestAPI.Services;
using System.Text;

namespace DocManSys_Tests
{
    [TestFixture]
    public class MinioClientService_Test
    {
        //private IMinioClient _fakeMinioClient;
        //private MinioClientService _minioClientService;

        //[SetUp]
        //public void Setup()
        //{
        //    _fakeMinioClient = A.Fake<IMinioClient>();
        //    _minioClientService = new MinioClientService(_fakeMinioClient);
        //}

        //[Test]
        //public async Task UploadFile_ShouldCallPutObjectAsync()
        //{
        //    // Arrange
        //    var file = A.Fake<IFormFile>();
        //    var fileName = "test.txt";
        //    var fileStream = new MemoryStream(Encoding.UTF8.GetBytes("dummy content"));
        //    A.CallTo(() => file.FileName).Returns(fileName);
        //    A.CallTo(() => file.OpenReadStream()).Returns(fileStream);
        //    A.CallTo(() => file.Length).Returns(fileStream.Length);

        //    // Act
        //    await _minioClientService.UploadFile(file);

        //    // Assert
        //    A.CallTo(() => _fakeMinioClient.PutObjectAsync)
        //        .MustHaveHappenedOnceExactly();
        //}

        //[Test]
        //public async Task DownloadFile_ShouldReturnFileStreamResult()
        //{
        //    // Arrange
        //    var fileName = "test.txt";
        //    var fileContent = "dummy content";
        //    var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
        //    A.CallTo(() => _fakeMinioClient.GetObjectAsync(A<GetObjectArgs>.That.Matches(args =>
        //        args.BucketName == "uploads" && args.ObjectName == fileName), A<Action<Stream>>.Ignored))
        //        .Invokes((GetObjectArgs args, Action<Stream> callback) => callback(memoryStream));

        //    // Act
        //    var result = await _minioClientService.DownloadFile(fileName) as FileStreamResult;

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.That(result.ContentType, Is.EqualTo("application/octet-stream"));
        //    Assert.That(result.FileDownloadName, Is.EqualTo(fileName));
        //}

        //[Test]
        //public async Task DeleteFile_ShouldReturnOkObjectResult()
        //{
        //    // Arrange
        //    var fileName = "test.txt";

        //    // Act
        //    var result = await _minioClientService.DeleteFile(fileName) as OkObjectResult;
        //    var resultValue = result.Value as string;

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.That(result.StatusCode, Is.EqualTo(200));
        //    Assert.That(resultValue, Is.EqualTo($"Datei '{fileName}' erfolgreich gelöscht.")) ;
        //}

        //[Test]
        //public async Task EnsureBucketExists_ShouldCreateBucketIfNotExists()
        //{
        //    // Arrange
        //    A.CallTo(() => _fakeMinioClient.BucketExistsAsync(A<BucketExistsArgs>.That.Matches(args =>
        //        args.BucketName == "uploads"))).Returns(false);

        //    // Act
        //    await _minioClientService.UploadFile(A.Fake<IFormFile>());

        //    // Assert
        //    A.CallTo(() => _fakeMinioClient.MakeBucketAsync(A<MakeBucketArgs>.That.Matches(args =>
        //        args.BucketName == "uploads"))).MustHaveHappenedOnceExactly();
        //}
        //[TearDown]
        //public void TearDown()
        //{
        //    _fakeMinioClient.Dispose();
        //}
    }
}

