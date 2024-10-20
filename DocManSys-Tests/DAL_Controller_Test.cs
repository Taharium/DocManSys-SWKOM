using DocManSys_DAL.Controllers;
using DocManSys_DAL.Entities;
using DocManSys_DAL.Repositories;
using FakeItEasy;

namespace DocManSys_Tests;

[TestFixture]
public class DocumentControllerDalTests
{
    private DocumentController _controller;
    private IDocumentRepository _fakeDocumentRepository;

    [SetUp]
    public void SetUp()
    {
        _fakeDocumentRepository = A.Fake<IDocumentRepository>();
        _controller = new DocumentController(_fakeDocumentRepository);
    }

    [Test]
    public async Task GetAllDocuments_ShouldReturnAllDocuments_WhenNoSearchTerm()
    {
        // Arrange
        var documents = new List<DocumentEntity>
        {
            new DocumentEntity { Title = "Doc1", Author = "Author1" },
            new DocumentEntity { Title = "Doc2", Author = "Author2" }
        };

        A.CallTo(() => _fakeDocumentRepository.GetAllDocumentsAsync())
            .Returns(documents);

        // Act
        var result = await _controller.GetAllDocuments();

        // Assert
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Title, Is.EqualTo("Doc2")); // Check reverse order
    }

    [Test]
    public async Task GetAllDocuments_ShouldReturnFilteredDocuments_WhenSearchTermProvided()
    {
        // Arrange
        var documents = new List<DocumentEntity>
        {
            new DocumentEntity { Title = "Document1", Author = "Author1" },
            new DocumentEntity { Title = "Document2", Author = "Author2" },
            new DocumentEntity { Title = "TestDocument", Author = "Author3" }
        };

        A.CallTo(() => _fakeDocumentRepository.GetAllDocumentsAsync())
            .Returns(documents);

        // Act
        var result = await _controller.GetAllDocuments("Test");

        // Assert
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Title, Is.EqualTo("TestDocument"));
    }

    [Test]
    public async Task GetAllDocuments_ShouldReturnEmpty_WhenNoDocumentsMatchSearchTerm()
    {
        // Arrange
        var documents = new List<DocumentEntity>
        {
            new DocumentEntity { Title = "Document1", Author = "Author1" },
            new DocumentEntity { Title = "Document2", Author = "Author2" }
        };

        A.CallTo(() => _fakeDocumentRepository.GetAllDocumentsAsync())
            .Returns(documents);

        // Act
        var result = await _controller.GetAllDocuments("NonExistentTerm");

        // Assert
        Assert.IsEmpty(result);
    }

    [Test]
    public async Task GetAllDocuments_ShouldHandleNullSearchTerm()
    {
        // Arrange
        var documents = new List<DocumentEntity>
        {
            new DocumentEntity { Title = "Document1", Author = "Author1" },
            new DocumentEntity { Title = "Document2", Author = "Author2" }
        };

        A.CallTo(() => _fakeDocumentRepository.GetAllDocumentsAsync())
            .Returns(documents);

        // Act
        var result = await _controller.GetAllDocuments(null);

        // Assert
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Title, Is.EqualTo("Document2")); // Check reverse order
    }
}