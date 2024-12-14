using System.Reflection;
using DocManSys_DAL.Data;
using DocManSys_DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocManSys_Tests;

public class DocumentContextTests {
        
        private DbContextOptions<DocumentContext> _options;
        private DocumentContext _context;

        [SetUp]
        public void Setup() {
            _options = new DbContextOptionsBuilder<DocumentContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            _context = new DocumentContext(_options);
        }

        [Test]
        public void Should_Have_Valid_Model_Configuration() {
            var modelBuilder = new ModelBuilder();
            var methodInfo = typeof(DocumentContext).GetMethod("OnModelCreating", BindingFlags.NonPublic | BindingFlags.Instance);
            
            methodInfo?.Invoke(_context, new object[] { modelBuilder });

            var entityType = modelBuilder.Model.FindEntityType(typeof(DocumentEntity));
            Assert.IsNotNull(entityType);
            
            Assert.That(entityType.GetTableName(), Is.EqualTo("Documents"));

            var titleProperty = entityType.GetProperty("Title");
            Assert.IsNotNull(titleProperty);
            Assert.That(titleProperty.GetMaxLength(), Is.EqualTo(150));

            var authorProperty = entityType.GetProperty("Author");
            Assert.IsNotNull(authorProperty);
            Assert.That(authorProperty.GetMaxLength(), Is.EqualTo(50));

            var ocrTextProperty = entityType.GetProperty("OcrText");
            Assert.IsNotNull(ocrTextProperty);
            Assert.That(ocrTextProperty.GetMaxLength(), Is.EqualTo(-1)); // For unlimited text
        }

        [Test]
        public async Task Should_Add_Document_To_Database() {
            var document = new DocumentEntity {
                Title = "Test Document",
                Author = "Test Author",
                OcrText = "Some text content"
            };
            
            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();

            var savedDocument = await _context.Documents
                                              .FirstOrDefaultAsync(d => d.Title == "Test Document");

            Assert.IsNotNull(savedDocument);
            Assert.That(savedDocument.Title, Is.EqualTo("Test Document"));
            Assert.That(savedDocument.Author, Is.EqualTo("Test Author"));
            Assert.That(savedDocument.OcrText, Is.EqualTo("Some text content"));
        }
        

        [TearDown]
        public void TearDown() {
            _context.Dispose();
        }
    }