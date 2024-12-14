using System.Net;
using DocManSys_RestAPI.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Response;

namespace DocManSys_Tests;

[TestFixture]
public class MinioClientServiceTests {
    private IMinioClient _fakeMinioClient;
    private MinioClientService _minioClientService;

    [SetUp]
    public void Setup() {
        _fakeMinioClient = A.Fake<IMinioClient>();

        A.CallTo(() => _fakeMinioClient.BucketExistsAsync(A<BucketExistsArgs>._, CancellationToken.None))
            .Returns(Task.FromResult(true));

        A.CallTo(() => _fakeMinioClient.PutObjectAsync(A<PutObjectArgs>._, CancellationToken.None))
            .Returns(Task.FromResult(new PutObjectResponse(
                HttpStatusCode.OK, // Status code
                "Upload Successful",
                new Dictionary<string, string>(),
                1024,
                "test-file.txt"
            )));

        _minioClientService = new MinioClientService(_fakeMinioClient);
    }

    [Test]
    public async Task UploadFile_ShouldCallPutObjectAsync_WhenFileIsUploaded() {
        // Arrange
        var file = A.Fake<IFormFile>();
        A.CallTo(() => file.FileName).Returns("test-file.txt");
        A.CallTo(() => file.Length).Returns(1024);
        A.CallTo(() => file.OpenReadStream()).Returns(new MemoryStream(new byte[1024])); // Mock file stream

        // Act
        await _minioClientService.UploadFile(file);

        // Assert
        A.CallTo(() => _fakeMinioClient.PutObjectAsync(A<PutObjectArgs>._, CancellationToken.None))
            .MustHaveHappenedOnceExactly();
    }


    [Test]
    public async Task DeleteFile_ShouldReturnOkResult_WhenFileIsDeleted() {
        // Arrange
        var fileName = "test-file.txt";
        A.CallTo(() => _fakeMinioClient.RemoveObjectAsync(A<RemoveObjectArgs>._, CancellationToken.None))
            .Returns(Task.CompletedTask); // Simulate successful deletion

        // Act
        var result = await _minioClientService.DeleteFile(fileName);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult); // Check that the result is OkObjectResult
    }

    [Test]
    public async Task DeleteFile_ShouldReturnErrorResult_WhenFileDeletionFails() {
        // Arrange
        var fileName = "test-file.txt";
        var exceptionMessage = "MinIO error";
        A.CallTo(() => _fakeMinioClient.RemoveObjectAsync(A<RemoveObjectArgs>._, CancellationToken.None))
            .Throws(new Exception(exceptionMessage)); // Simulate an exception during deletion

        // Act
        var result = await _minioClientService.DeleteFile(fileName);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult); // Check that the result is ObjectResult
        //Assert.That(objectResult.Value, Is.EqualTo($"Fehler beim Löschen der Datei: {exceptionMessage}")); // Check error message
    }

    [Test]
    public async Task EnsureBucketExists_ShouldNotCreateBucket_WhenBucketExists() {
        // Arrange
        A.CallTo(() => _fakeMinioClient.BucketExistsAsync(A<BucketExistsArgs>._, CancellationToken.None))
            .Returns(Task.FromResult(true)); // Simulate the bucket existing

        // Act
        await _minioClientService.EnsureBucketExists();

        // Assert
        // Ensure that MakeBucketAsync was NOT called, as the bucket exists
        A.CallTo(() => _fakeMinioClient.MakeBucketAsync(A<MakeBucketArgs>._, CancellationToken.None))
            .MustNotHaveHappened();
    }


    [TearDown]
    public void TearDown() {
        _fakeMinioClient.Dispose();
    }
}