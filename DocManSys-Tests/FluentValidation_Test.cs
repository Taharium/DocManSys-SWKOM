using DocManSys_RestAPI.Models;
using FluentValidation.TestHelper;

namespace DocManSys_Tests;

public class FluentValidationTest {
    private DocumentValidator _documentValidator;
    
    [SetUp]
    public void Setup() {
        _documentValidator = new DocumentValidator();
    }

    [Test]
    public void DocumentTitleEmpty() {
        var doc = new Document() {
            Title = "",
            Author = "hfdk"
        };
        var result =  _documentValidator.TestValidate(doc);

        result.ShouldHaveValidationErrorFor(d => d.Title);
    }
    [Test]
    public void DocumentAuthorEmpty() {
        var doc = new Document() {
            Title = "fjkglkndf",
            Author = ""
        };
        var result =  _documentValidator.TestValidate(doc);

        result.ShouldHaveValidationErrorFor(d => d.Author);
    }
    [Test]
    public void DocumentAuthorAndTitleEmpty() {
        var doc = new Document() {
            Title = "",
            Author = ""
        };
        var result =  _documentValidator.TestValidate(doc);

        result.ShouldHaveValidationErrorFor(d => d.Author);
        result.ShouldHaveValidationErrorFor(d => d.Title);
    }
    [Test]
    public void DocumentAuthorAndTitleNotEmpty() {
        var doc = new Document() {
            Title = "djfk",
            Author = "fn,dm"
        };
        var result =  _documentValidator.TestValidate(doc);

        result.ShouldNotHaveValidationErrorFor(d => d.Author);
        result.ShouldNotHaveValidationErrorFor(d => d.Title);
    }
    
    [Test]
    public void DocumentTitleTooLong() {
        var doc = new Document() {
            Title = "hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjj",
            Author = "hfdk"
        };
        var result =  _documentValidator.TestValidate(doc);

        result.ShouldHaveValidationErrorFor(d => d.Title);
    }
    [Test]
    public void DocumentAuthorTooLong() {
        var doc = new Document() {
            Title = "dfjd",
            Author = "hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh"
        };
        var result =  _documentValidator.TestValidate(doc);

        result.ShouldHaveValidationErrorFor(d => d.Author);
    }
}