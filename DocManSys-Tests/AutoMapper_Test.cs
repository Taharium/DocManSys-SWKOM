using AutoMapper;
using DocManSys_DAL.Entities;
using DocManSys_RestAPI.Mappings;
using DocManSys_RestAPI.Models;

namespace DocManSys_Tests;

public class AutoMapper_Test {
    private IMapper _mapper;
    
    [SetUp]
    public void Setup() {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Test]
    public void Document_to_DocumentEntity() {
        var item = new Document() {
            Id = 4,
            Title = "hfdjkhs",
            Author = "jkfdhkd",
        };
        
        var actual = _mapper.Map<Document, DocumentEntity>(item);

        Assert.That(item.Title, Is.EqualTo(actual.Title));
        Assert.That(item.Id, Is.EqualTo(actual.Id));
        Assert.That(item.Author, Is.EqualTo(actual.Author));
    }
    [Test]
    public void DocumentEntity_to_Document() {
        var item = new DocumentEntity() {
            Id = 4,
            Title = "hfdjkhs",
            Author = "jkfdhkd",
        };
        
        var actual = _mapper.Map<DocumentEntity, Document>(item);

        Assert.That(item.Title, Is.EqualTo(actual.Title));
        Assert.That(item.Id, Is.EqualTo(actual.Id));
        Assert.That(item.Author, Is.EqualTo(actual.Author));
    }
}