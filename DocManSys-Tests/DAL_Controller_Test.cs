using DocManSys_DAL.Controllers;
using DocManSys_DAL.Entities;
using DocManSys_DAL.Repositories;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocManSys_Tests;

[TestFixture]
public class DocumentControllerDalTestGetAllDocuments {
    private DocumentControllerDal _controllerDal;
    private IDocumentRepository _fakeDocumentRepository;
    private ILogger<DocumentControllerDal> _logger;

    [SetUp]
    public void SetUp() {
        _fakeDocumentRepository = A.Fake<IDocumentRepository>();
        _logger = A.Fake<ILogger<DocumentControllerDal>>();
        _controllerDal = new DocumentControllerDal(_fakeDocumentRepository, _logger);
    }

    [Test]
    public async Task GetAllDocuments_ShouldReturnAllDocuments_WhenNoSearchTerm() {
        // Arrange
        var documents = new List<DocumentEntity> {
            new DocumentEntity { Title = "Doc1", Author = "Author1" },
            new DocumentEntity { Title = "Doc2", Author = "Author2" }
        };

        A.CallTo(() => _fakeDocumentRepository.GetAllDocumentsAsync())
            .Returns(documents);

        // Act
        var result = await _controllerDal.GetAllDocuments();

        // Assert
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Title, Is.EqualTo("Doc2")); // Check reverse order
    }

    [Test]
    public async Task GetAllDocuments_ShouldReturnFilteredDocuments_WhenSearchTermProvided() {
        // Arrange
        var documents = new List<DocumentEntity> {
            new DocumentEntity { Title = "Document1", Author = "Author1" },
            new DocumentEntity { Title = "Document2", Author = "Author2" },
            new DocumentEntity { Title = "TestDocument", Author = "Author3" }
        };

        A.CallTo(() => _fakeDocumentRepository.GetAllDocumentsAsync())
            .Returns(documents);

        // Act
        var result = await _controllerDal.GetAllDocuments("Test");

        // Assert
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Title, Is.EqualTo("TestDocument"));
    }

    [Test]
    public async Task GetAllDocuments_ShouldReturnEmpty_WhenNoDocumentsMatchSearchTerm() {
        // Arrange
        var documents = new List<DocumentEntity> {
            new DocumentEntity { Title = "Document1", Author = "Author1" },
            new DocumentEntity { Title = "Document2", Author = "Author2" }
        };

        A.CallTo(() => _fakeDocumentRepository.GetAllDocumentsAsync())
            .Returns(documents);

        // Act
        var result = await _controllerDal.GetAllDocuments("NonExistentTerm");

        // Assert
        Assert.IsEmpty(result);
    }

    [Test]
    public async Task GetAllDocuments_ShouldHandleNullSearchTerm() {
        // Arrange
        var documents = new List<DocumentEntity> {
            new DocumentEntity { Title = "Document1", Author = "Author1" },
            new DocumentEntity { Title = "Document2", Author = "Author2" }
        };

        A.CallTo(() => _fakeDocumentRepository.GetAllDocumentsAsync())
            .Returns(documents);

        // Act
        var result = await _controllerDal.GetAllDocuments(null!);

        // Assert
        var documentEntities = result as DocumentEntity[] ?? result.ToArray();
        Assert.That(documentEntities.Count(), Is.EqualTo(2));
        Assert.That(documentEntities.First().Title, Is.EqualTo("Document2")); // Check reverse order
    }
}

[TestFixture]
public class DocumentControllerTests {
    private IDocumentRepository _fakeDocumentRepository;
    private ILogger<DocumentControllerDal> _fakeLogger;
    private DocumentControllerDal _controllerDal;

    [SetUp]
    public void SetUp() {
        _fakeDocumentRepository = A.Fake<IDocumentRepository>();
        _fakeLogger = A.Fake<ILogger<DocumentControllerDal>>();
        _controllerDal = new DocumentControllerDal(_fakeDocumentRepository, _fakeLogger);
    }

    [Test]
    public async Task GetDocumentById_ValidId_ReturnsDocument() {
        // Arrange
        var documentId = 1;
        var expectedDocument = new DocumentEntity { Id = documentId, Title = "Test Document" };
        A.CallTo(() => _fakeDocumentRepository.GetDocumentByIdAsync(documentId))
            .Returns(Task.FromResult<DocumentEntity?>(expectedDocument));

        // Act
        var result = await _controllerDal.GetDocumentById(documentId);

        // Assert
        Assert.NotNull(result);
        Assert.That(result?.Id, Is.EqualTo(expectedDocument.Id));
        Assert.That(result?.Title, Is.EqualTo(expectedDocument.Title));
        A.CallTo(() => _fakeDocumentRepository.GetDocumentByIdAsync(documentId))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task GetDocumentById_InvalidId_ReturnsNull() {
        // Arrange
        var documentId = 99; // Non-existent ID
        A.CallTo(() => _fakeDocumentRepository.GetDocumentByIdAsync(documentId))
            .Returns(Task.FromResult<DocumentEntity?>(null));

        // Act
        var result = await _controllerDal.GetDocumentById(documentId);

        // Assert
        Assert.IsNull(result);
        A.CallTo(() => _fakeDocumentRepository.GetDocumentByIdAsync(documentId))
            .MustHaveHappenedOnceExactly();
    }
}

[TestFixture]
public class DocumentControllerDalTestsPost
{
    private IDocumentRepository _fakeDocumentRepository;
    private DocumentControllerDal _controller;

    [SetUp]
    public void SetUp()
    {
        _fakeDocumentRepository = A.Fake<IDocumentRepository>();
        _controller = new DocumentControllerDal(_fakeDocumentRepository, A.Fake<Microsoft.Extensions.Logging.ILogger<DocumentControllerDal>>());
    }

    [Test]
    public async Task AddDocument_ValidDocument_ReturnsOk()
    {
        // Arrange
        var documentEntity = new DocumentEntity
        {
            Id = 1,
            Title = "Valid Title",
            Author = "Author Name",
            OcrText = "OCR Text"
        };

        // Act
        var result = await _controller.AddDocument(documentEntity);

        // Assert
        Assert.IsInstanceOf<OkResult>(result);
        A.CallTo(() => _fakeDocumentRepository.AddDocumentAsync(documentEntity)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task AddDocument_EmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        var documentEntity = new DocumentEntity
        {
            Id = 2,
            Title = "", // Invalid empty title
            Author = "Author Name",
            OcrText = "OCR Text"
        };

        // Act
        var result = await _controller.AddDocument(documentEntity);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestResult);

        A.CallTo(() => _fakeDocumentRepository.AddDocumentAsync(A<DocumentEntity>.Ignored)).MustNotHaveHappened();
    }
}