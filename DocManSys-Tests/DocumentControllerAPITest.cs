using System.Net;
using System.Net.Http.Json;
using AutoMapper;
using DocManSys_DAL.Entities;
using DocManSys_RestAPI.Controllers;
using DocManSys_RestAPI.Models;
using DocManSys_RestAPI.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocManSys_Tests;

public class MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken) {
        return Task.FromResult(handler(request));
    }
}

public class DocumentControllerApiTests {
    private DocumentController _controller;
    private IHttpClientFactory _clientFactory;
    private ILogger<DocumentController> _logger;
    private IMapper _mapper;
    private IMessageQueueService _messageQueueService;
    private IMinioClientService _minioClientService;
    private IElasticsearchService _elasticsearchService;

    [SetUp]
    public void Setup() {
        // Mock dependencies
        _clientFactory = A.Fake<IHttpClientFactory>();
        _logger = A.Fake<ILogger<DocumentController>>();
        _mapper = A.Fake<IMapper>();
        _messageQueueService = A.Fake<IMessageQueueService>();
        _minioClientService = A.Fake<IMinioClientService>();
        _elasticsearchService = A.Fake<IElasticsearchService>();

        // Create a mock HttpMessageHandler and HttpClient
        var mockHttpMessageHandler = new MockHttpMessageHandler(request => {
            // Check the URI and return a different response based on it
            if (request.RequestUri?.AbsolutePath == "/api/DAL/document/1") {
                // Simulate a valid document response
                var documentEntity = new DocumentEntity {
                    Id = 1,
                    // Add other properties for the document entity as needed
                };

                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = JsonContent.Create(documentEntity)
                };
            }

            // Simulate a 404 for any other path
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var httpClient = new HttpClient(mockHttpMessageHandler) {
            BaseAddress = new Uri("http://localhost:8082")
        };

        A.CallTo(() => _mapper.Map<Document>(A<DocumentEntity>._))
            .Returns(new Document {
                Id = 1,
                Title = "document.pdf",
                Author = "test",
                OcrText = "TEST"
            });
        A.CallTo(() => _mapper.Map<DocumentEntity>(A<Document>._))
            .Returns(new DocumentEntity() {
                Id = 1,
                Title = "document.pdf",
                Author = "test",
                OcrText = "TEST"
            });

        // Make the HttpClient available to the controller
        A.CallTo(() => _clientFactory.CreateClient(A<string>._)).Returns(httpClient);

        // Setup the controller
        _controller = new DocumentController(
            _clientFactory,
            _logger,
            _mapper,
            _messageQueueService,
            _minioClientService,
            _elasticsearchService
        );
    }

    [Test]
    public async Task GetDocument_ReturnsOkResult_WhenDocumentExists() {
        // Arrange
        int documentId = 1;
        var documentEntity = new DocumentEntity {
            Id = documentId,
            // Add other properties for the document entity as needed
        };

        var documentModel = new Document {
            Id = documentId,
            // Add other properties for the document model as needed
        };

        // Map the entity to the model
        A.CallTo(() => _mapper.Map<Document>(documentEntity))
            .Returns(documentModel);

        // Act
        var result = await _controller.GetDocument(documentId);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task GetDocument_ReturnsNotFound_WhenDocumentDoesNotExist() {
        // Arrange
        int documentId = 999; // Simulate a non-existing document ID

        // Act
        var result = await _controller.GetDocument(documentId) as ObjectResult;

        // Assert
        Assert.That(result?.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GetDocument_ReturnsError_WhenHttpRequestFails() {
        // Arrange
        int documentId = 1;

        // Simulate failure by returning a 500 Internal Server Error
        var mockHttpMessageHandler =
            new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var httpClient = new HttpClient(mockHttpMessageHandler) {
            BaseAddress = new Uri("http://localhost:8082")
        };

        A.CallTo(() => _clientFactory.CreateClient(A<string>._)).Returns(httpClient);

        // Act
        var result = await _controller.GetDocument(documentId);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task GetDocuments_ReturnsOkResult_WhenDocumentsExist() {
        // Arrange
        var documentEntities = new List<DocumentEntity> {
            new DocumentEntity { Id = 1, /* Add properties as needed */ },
            new DocumentEntity { Id = 2, /* Add properties as needed */ }
        };

        var documentModels = new List<Document> {
            new Document { Id = 1, /* Map the properties from DocumentEntity */ },
            new Document { Id = 2, /* Map the properties from DocumentEntity */ }
        };

        // Setup mock response for HttpClient
        var mockHttpMessageHandler = new MockHttpMessageHandler(request => {
            if (request.RequestUri?.AbsolutePath == "/api/DAL/document") {
                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = JsonContent.Create(documentEntities)
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var httpClient = new HttpClient(mockHttpMessageHandler) {
            BaseAddress = new Uri("http://localhost:8082")
        };

        A.CallTo(() => _clientFactory.CreateClient(A<string>._)).Returns(httpClient);

        // Map the entities to models
        A.CallTo(() => _mapper.Map<IEnumerable<Document>>(documentEntities)).Returns(documentModels);

        // Act
        var result = await _controller.GetDocuments();

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task GetDocuments_ReturnsInternalServerError_WhenHttpRequestFails() {
        // Arrange
        var mockHttpMessageHandler =
            new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var httpClient = new HttpClient(mockHttpMessageHandler) {
            BaseAddress = new Uri("http://localhost:8082")
        };

        A.CallTo(() => _clientFactory.CreateClient(A<string>._)).Returns(httpClient);

        // Act
        var result = await _controller.GetDocuments();

        // Assert
        Assert.IsInstanceOf<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task PutDocument_ReturnsNoContent_WhenDocumentIsUpdatedSuccessfully() {
        // Arrange
        int documentId = 1;
        var document = new Document {
            Id = documentId,
            OcrText = "Updated OcrText"
        };

        var documentEntity = new DocumentEntity {
            Id = documentId,
            OcrText = "Updated OcrText"
        };

        // Set up mock response for HttpClient (simulating a successful PUT request)
        var mockHttpMessageHandler = new MockHttpMessageHandler(request => {
            if (request.RequestUri?.AbsolutePath == $"/api/DAL/document/{documentId}") {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var httpClient = new HttpClient(mockHttpMessageHandler) {
            BaseAddress = new Uri("http://localhost:8082")
        };

        A.CallTo(() => _clientFactory.CreateClient(A<string>._)).Returns(httpClient);
        A.CallTo(() => _mapper.Map<DocumentEntity>(document)).Returns(documentEntity);

        // Act
        var result = await _controller.PutDocument(documentId, document);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
    }

    [Test]
    public async Task PutDocument_ReturnsError_WhenHttpRequestFails() {
        // Arrange
        int documentId = 1;
        var document = new Document {
            Id = documentId,
            OcrText = "Updated OcrText"
        };

        var documentEntity = new DocumentEntity {
            Id = documentId,
            OcrText = "Updated OcrText"
        };

        // Simulate failure by returning a 500 Internal Server Error
        var mockHttpMessageHandler =
            new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var httpClient = new HttpClient(mockHttpMessageHandler) {
            BaseAddress = new Uri("http://localhost:8082")
        };

        A.CallTo(() => _clientFactory.CreateClient(A<string>._)).Returns(httpClient);
        A.CallTo(() => _mapper.Map<DocumentEntity>(document)).Returns(documentEntity);

        // Act
        var result = await _controller.PutDocument(documentId, document);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task UploadFile_ReturnsBadRequest_WhenNoFileUploaded() {
        // Arrange
        int documentId = 1;
        IFormFile? documentFile = null;

        // Act
        var result = await _controller.UploadFile(documentId, documentFile);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.Not.Null);
    }

    [Test]
    public async Task UploadFile_ReturnsBadRequest_WhenNonPdfFileUploaded() {
        // Arrange
        int documentId = 1;
        var mockFile = A.Fake<IFormFile>();
        A.CallTo(() => mockFile.FileName).Returns("document.txt");

        // Act
        var result = await _controller.UploadFile(documentId, mockFile);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.Not.Null);
    }

    [Test]
    public async Task UploadFile_ReturnsStatusCode400_WhenLengthIsZero() {
        // Arrange
        int documentId = 1;
        var mockFile = A.Fake<IFormFile>();
        A.CallTo(() => mockFile.FileName).Returns("document.pdf");

        // Simulate failure in the PUT request to update the document
        var mockHttpMessageHandler =
            new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var httpClient = new HttpClient(mockHttpMessageHandler) { BaseAddress = new Uri("http://localhost:8082") };
        A.CallTo(() => _clientFactory.CreateClient(A<string>._)).Returns(httpClient);

        // Act
        var result = await _controller.UploadFile(documentId, mockFile);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UploadFile_ReturnsNotFound_WhenDocumentNotFound() {
        // Arrange
        int documentId = 10;
        var mockFile = A.Fake<IFormFile>();
        A.CallTo(() => mockFile.FileName).Returns("document.pdf");
        A.CallTo(() => mockFile.Length).Returns(12);

        // Simulate a failure in retrieving the document
        var mockHttpMessageHandler = new MockHttpMessageHandler(request => {
            // Check the URI and return a different response based on it
            if (request.RequestUri?.AbsolutePath == "/api/DAL/document/1") {
                // Simulate a valid document response
                var documentEntity = new DocumentEntity {
                    Id = 1,
                    Title = "document.pdf",
                    Author = "test",
                    OcrText = "TEST"
                    // Add other properties for the document entity as needed
                };

                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = JsonContent.Create(documentEntity)
                };
            }

            // Simulate a 404 for any other path
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var httpClient = new HttpClient(mockHttpMessageHandler) { BaseAddress = new Uri("http://localhost:8082") };
        A.CallTo(() => _clientFactory.CreateClient(A<string>._)).Returns(httpClient);

        // Act
        var result = await _controller.UploadFile(documentId, mockFile);

        // Assert
        Assert.IsInstanceOf<NotFoundObjectResult>(result);
    }

    [Test]
    public async Task UploadFile_ReturnsOk_WhenFileUploadedSuccessfully() {
        // Arrange
        int documentId = 1;
        var mockFile = A.Fake<IFormFile>();
        A.CallTo(() => mockFile.FileName).Returns("document.pdf");
        A.CallTo(() => mockFile.Length).Returns(12);
        // Simulate success for all HTTP requests
        var mockHttpMessageHandler = new MockHttpMessageHandler(request => {
            // Check the URI and return a different response based on it
            if (request.RequestUri?.AbsolutePath == "/api/DAL/document/1") {
                // Simulate a valid document response
                var documentEntity = new DocumentEntity {
                    Id = 1,
                    Title = "document.pdf",
                    Author = "test",
                    OcrText = "TEST"
                    // Add other properties for the document entity as needed
                };

                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = JsonContent.Create(documentEntity)
                };
            }

            // Simulate a 404 for any other path
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var httpClient = new HttpClient(mockHttpMessageHandler) { BaseAddress = new Uri("http://localhost:8082") };
        A.CallTo(() => _clientFactory.CreateClient(A<string>._)).Returns(httpClient);

        // Act
        var result = await _controller.UploadFile(documentId, mockFile);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UploadFile_Returns400_WhenDocumentIsNotAPDF() {
        // Arrange
        int documentId = 10;
        var mockFile = A.Fake<IFormFile>();
        A.CallTo(() => mockFile.FileName).Returns("document.txt");
        A.CallTo(() => mockFile.Length).Returns(12);

        // Simulate a failure in retrieving the document
        var mockHttpMessageHandler = new MockHttpMessageHandler(request => {
            // Check the URI and return a different response based on it
            if (request.RequestUri?.AbsolutePath == "/api/DAL/document/1") {
                // Simulate a valid document response
                var documentEntity = new DocumentEntity {
                    Id = 1,
                    Title = "document.pdf",
                    Author = "test",
                    OcrText = "TEST"
                    // Add other properties for the document entity as needed
                };

                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = JsonContent.Create(documentEntity)
                };
            }

            // Simulate a 404 for any other path
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var httpClient = new HttpClient(mockHttpMessageHandler) { BaseAddress = new Uri("http://localhost:8082") };
        A.CallTo(() => _clientFactory.CreateClient(A<string>._)).Returns(httpClient);

        // Act
        var result = await _controller.UploadFile(documentId, mockFile);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public async Task PostDocument_ReturnsStatusCode500_WhenDocumentSaveFails() {
        // Arrange
        var document = new Document {
            Id = 1,
            Title = "document.pdf",
            Author = "test",
            OcrText = "TEST"
        };

        // Simulate an HTTP failure when saving the document
        var mockHttpMessageHandler = new MockHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.InternalServerError) {
                Content = JsonContent.Create(new { message = "Failed to save document to the database" })
            });

        var httpClient = new HttpClient(mockHttpMessageHandler) { BaseAddress = new Uri("http://localhost:8082") };
        A.CallTo(() => _clientFactory.CreateClient(A<string>._)).Returns(httpClient);

        // Act
        var result = await _controller.PostDocument(document);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500)); // Check if the status is 500
        Assert.That(objectResult?.Value, Is.Not.Null);
    }

    [Test]
    public async Task DeleteDocument_ReturnsNoContent_WhenDocumentDeletedSuccessfully() {
        // Arrange
        var documentId = 1;

        // Act
        var result = await _controller.DeleteDocument(documentId);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result); // Expect NoContent (204)
    }


    [Test]
    public async Task DeleteDocument_ReturnsStatusCode500_WhenDatabaseDeleteFails() {
        // Arrange
        var documentId = 1;

        // Simulate a failed delete from the database (HTTP 500 Internal Server Error)
        var mockHttpMessageHandler =
            new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var httpClient = new HttpClient(mockHttpMessageHandler) { BaseAddress = new Uri("http://localhost:8082") };
        A.CallTo(() => _clientFactory.CreateClient(A<string>._)).Returns(httpClient);

        // Act
        var result = await _controller.DeleteDocument(documentId);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(result); // Expect ObjectResult
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500)); // Expect status code 500 (Internal Server Error)
    }

    [Test]
    public void DownloadFile_ReturnsFile_WhenFileExists() {
        // Arrange
        var fileName = "testfile.pdf";
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", fileName);

        // Create the file in a test directory
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        if (!Directory.Exists(directoryPath)) {
            Directory.CreateDirectory(directoryPath);
        }

        if (!File.Exists(filePath)) {
            File.WriteAllText(filePath, "This is a test file.");
        }

        // Act
        var result = _controller.DownloadFile(fileName) as PhysicalFileResult;

        // Assert
        Assert.IsNotNull(result); // Ensure we get a result back
        Assert.That(result.FileName, Is.EqualTo(filePath)); // Ensure the correct file path
        Assert.That(result.ContentType, Is.EqualTo("application/octet-stream")); // Ensure the content type is correct
    }

    [Test]
    public void DownloadFile_ReturnsNotFound_WhenFileDoesNotExist() {
        // Arrange
        var fileName = "nonexistentfile.pdf";
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", fileName);

        // Act
        var result = _controller.DownloadFile(fileName) as NotFoundObjectResult;

        // Assert
        Assert.IsNotNull(result); // Ensure we get a NotFound result back
        Assert.That(result.StatusCode, Is.EqualTo(404)); // Ensure the status code is 404
        Assert.That(result.Value?.ToString(),
            Is.EqualTo($"{{ message = File '{filePath}' not found. }}")); // Ensure the error message is correct
    }
}