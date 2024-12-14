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
        A.CallTo(() => _fakeDocumentRepository.GetDocumentByIdAsync(documentId))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task GetDocumentById_InvalidId_ReturnsNotFound() {
        // Arrange
        var documentId = 99; // Non-existent ID
        A.CallTo(() => _fakeDocumentRepository.GetDocumentByIdAsync(documentId))
            .Returns(Task.FromResult<DocumentEntity?>(null));

        // Act
        var result = await _controllerDal.GetDocumentById(documentId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<NotFoundObjectResult>(result);
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
        Assert.IsInstanceOf<OkObjectResult>(result);
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

[TestFixture]
public class UpdateDocumentTests
{
    private IDocumentRepository _documentRepositoryFake;
    private ILogger<DocumentControllerDal> _loggerFake;
    private DocumentControllerDal _controller;

    [SetUp]
    public void SetUp()
    {
        _documentRepositoryFake = A.Fake<IDocumentRepository>();
        _loggerFake = A.Fake<ILogger<DocumentControllerDal>>();
        _controller = new DocumentControllerDal(_documentRepositoryFake, _loggerFake);
    }

    [Test]
    public async Task UpdateDocument_DocumentNotFound_ReturnsNotFound()
    {
        // Arrange
        var documentEntity = new DocumentEntity { Id = 1, Title = "Test Document" };
        A.CallTo(() => _documentRepositoryFake.GetDocumentByIdAsync(documentEntity.Id))!.Returns(Task.FromResult<DocumentEntity>(null));

        // Act
        var result = await _controller.UpdateDocument(documentEntity);

        // Assert
        Assert.IsInstanceOf<NotFoundResult>(result);
    }

    [Test]
    public async Task UpdateDocument_DocumentFound_UpdatesAndReturnsOk()
    {
        // Arrange
        var documentEntity = new DocumentEntity { Id = 1, Title = "Updated Title" };
        var existingDocument = new DocumentEntity { Id = 1, Title = "Old Title" };

        A.CallTo(() => _documentRepositoryFake.GetDocumentByIdAsync(documentEntity.Id))!.Returns(Task.FromResult(existingDocument));
        A.CallTo(() => _documentRepositoryFake.UpdateDocumentAsync(A<DocumentEntity>.Ignored)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateDocument(documentEntity);

        // Assert
        Assert.IsInstanceOf<OkResult>(result);
        Assert.That(existingDocument.Title, Is.EqualTo(documentEntity.Title));
        A.CallTo(() => _documentRepositoryFake.UpdateDocumentAsync(existingDocument)).MustHaveHappenedOnceExactly();
    }
}

[TestFixture]
public class PutAsyncDocumentTests
{
    private IDocumentRepository _documentRepositoryFake;
    private ILogger<DocumentControllerDal> _loggerFake;
    private DocumentControllerDal _controller;

    [SetUp]
    public void SetUp()
    {
        _documentRepositoryFake = A.Fake<IDocumentRepository>();
        _loggerFake = A.Fake<ILogger<DocumentControllerDal>>();
        _controller = new DocumentControllerDal(_documentRepositoryFake, _loggerFake);
    }

    [Test]
    public async Task PutAsync_DocumentNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = 1;
        var item = new DocumentEntity { Id = id, Title = "Test Document", Author = "Author", OcrText = "OCR Text" };
        A.CallTo(() => _documentRepositoryFake.GetDocumentByIdAsync(id))!.Returns(Task.FromResult<DocumentEntity>(null));

        // Act
        var result = await _controller.PutAsync(id, item);

        // Assert
        Assert.IsInstanceOf<NotFoundResult>(result);
    }

    [Test]
    public async Task PutAsync_DocumentFound_UpdatesAndReturnsNoContent()
    {
        // Arrange
        var id = 1;
        var item = new DocumentEntity { Id = id, Title = "Updated Title", Author = "New Author", OcrText = "Updated OCR Text" };
        var existingDocument = new DocumentEntity { Id = id, Title = "Old Title", Author = "Old Author", OcrText = "Old OCR Text" };

        A.CallTo(() => _documentRepositoryFake.GetDocumentByIdAsync(id))!.Returns(Task.FromResult(existingDocument));
        A.CallTo(() => _documentRepositoryFake.UpdateDocumentAsync(A<DocumentEntity>.Ignored)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.PutAsync(id, item);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
        Assert.That(existingDocument.Title, Is.EqualTo(item.Title));
        Assert.That(existingDocument.Author, Is.EqualTo(item.Author));
        Assert.That(existingDocument.OcrText, Is.EqualTo(item.OcrText));
        A.CallTo(() => _documentRepositoryFake.UpdateDocumentAsync(existingDocument)).MustHaveHappenedOnceExactly();
    }
}

[TestFixture]
public class DeleteDocumentTests
{
    private IDocumentRepository _documentRepositoryFake;
    private ILogger<DocumentControllerDal> _loggerFake;
    private DocumentControllerDal _controller;

    [SetUp]
    public void SetUp()
    {
        _documentRepositoryFake = A.Fake<IDocumentRepository>();
        _loggerFake = A.Fake<ILogger<DocumentControllerDal>>();
        _controller = new DocumentControllerDal(_documentRepositoryFake, _loggerFake);
    }

    [Test]
    public async Task DeleteDocument_DocumentNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = 1;
        A.CallTo(() => _documentRepositoryFake.GetDocumentByIdAsync(id))!.Returns(Task.FromResult<DocumentEntity>(null));

        // Act
        var result = await _controller.DeleteDocument(id);

        // Assert
        Assert.IsInstanceOf<NotFoundResult>(result);
    }

    [Test]
    public async Task DeleteDocument_DocumentFound_DeletesAndReturnsNoContent()
    {
        // Arrange
        var id = 1;
        var existingDocument = new DocumentEntity { Id = id, Title = "Old Title", Author = "Old Author", OcrText = "Old OCR Text" };

        A.CallTo(() => _documentRepositoryFake.GetDocumentByIdAsync(id))!.Returns(Task.FromResult(existingDocument));
        A.CallTo(() => _documentRepositoryFake.DeleteDocumentAsync(id)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteDocument(id);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
        A.CallTo(() => _documentRepositoryFake.DeleteDocumentAsync(id)).MustHaveHappenedOnceExactly();
    }
}
