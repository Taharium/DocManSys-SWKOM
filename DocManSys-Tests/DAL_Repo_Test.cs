using DocManSys_DAL.Data;
using DocManSys_DAL.Entities;
using DocManSys_DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DocManSys_Tests;

[TestFixture]
public class DocumentRepositoryTests {

    private IDocumentRepository _documentRepository;
    private DocumentContext _context;

    [SetUp]
    public void Setup() {
        var options = new DbContextOptionsBuilder<DocumentContext>()
                      .UseInMemoryDatabase("TestDatabase")
                      .Options;
        _context = new DocumentContext(options);

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _documentRepository = new DocumentRepository(_context);
    }

    [Test]
    public async Task GetAllDocumentsAsync_Should_Return_All_Documents() {
        var documents = new List<DocumentEntity> {
            new DocumentEntity { Id = 1, Title = "Document 1", Author = "Author 1" },
            new DocumentEntity { Id = 2, Title = "Document 2", Author = "Author 2" }
        };
        await _context.Documents.AddRangeAsync(documents);
        await _context.SaveChangesAsync();

        var result = await _documentRepository.GetAllDocumentsAsync();

        Assert.IsNotNull(result);
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Title, Is.EqualTo("Document 1"));
        Assert.That(result.First().Author, Is.EqualTo("Author 1"));
    }

    [Test]
    public async Task GetAllDocumentsAsync_Should_Return_Empty_List_When_No_Documents() {
        var result = await _documentRepository.GetAllDocumentsAsync();

        Assert.IsNotNull(result);
        Assert.That(result.Count(), Is.EqualTo(0));
    }
    
    [Test]
    public async Task GetDocumentByIdAsync_Should_Return_Document_When_Found() {
        var document = new DocumentEntity { Id = 1, Title = "Document 1", Author = "Author 1" };
        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();

        var result = await _documentRepository.GetDocumentByIdAsync(1);

        Assert.IsNotNull(result);
        Assert.That(result?.Id, Is.EqualTo(1));
        Assert.That(result?.Title, Is.EqualTo("Document 1"));
        Assert.That(result?.Author, Is.EqualTo("Author 1"));
    }

    [Test]
    public async Task GetDocumentByIdAsync_Should_Return_Null_When_Not_Found() {
        var result = await _documentRepository.GetDocumentByIdAsync(999);

        Assert.IsNull(result);
    }
    
    [Test]
    public async Task AddDocumentAsync_Should_Add_Document() {
        // Act
        var document = new DocumentEntity { Id = 1, Title = "Document 1", Author = "Author 1" };
        await _documentRepository.AddDocumentAsync(document);

        // Assert
        var result = await _context.Documents.FindAsync(1);
        Assert.IsNotNull(result);
        Assert.That(result?.Id, Is.EqualTo(1));
        Assert.That(result?.Title, Is.EqualTo("Document 1"));
        Assert.That(result?.Author, Is.EqualTo("Author 1"));
    }
    
    [Test]
    public async Task DeleteDocumentAsync_Should_Delete_Document_When_Found() {
        // Arrange
        var document = new DocumentEntity { Id = 1, Title = "Document 1", Author = "Author 1" };
        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();

        // Act
        await _documentRepository.DeleteDocumentAsync(1);

        // Assert
        var result = await _context.Documents.FindAsync(1);
        Assert.IsNull(result);
    }

    [Test]
    public async Task DeleteDocumentAsync_Should_Do_Nothing_When_Not_Found() {
        // Act
        await _documentRepository.DeleteDocumentAsync(999);

        // Assert
        var result = await _context.Documents.FindAsync(999);
        Assert.IsNull(result);
    }
    
    [Test]
    public async Task UpdateDocumentAsync_Should_Update_Existing_Document() {
        // Arrange
        var document = new DocumentEntity { Id = 1, Title = "Document 1", Author = "Author 1" };
        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();

        // Act
        document.Title = "Updated Document";
        await _documentRepository.UpdateDocumentAsync(document);

        // Assert
        var result = await _context.Documents.FindAsync(1);
        Assert.IsNotNull(result);
        Assert.That(result?.Title, Is.EqualTo("Updated Document"));
        Assert.That(result?.Author, Is.EqualTo("Author 1"));
    }

    [TearDown]
    public void TearDown() {
        _context.Dispose();
    }
}
    