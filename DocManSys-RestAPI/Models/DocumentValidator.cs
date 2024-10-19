using FluentValidation;

namespace DocManSys_RestAPI.Models;

public class DocumentValidator : AbstractValidator<Document> {
    public DocumentValidator() {
        RuleFor(doc => doc.Title)
            .NotEmpty().WithMessage("Please enter a Document Title!")
            .MaximumLength(150).WithMessage("The task name must not exceed 150 characters!");
        
        RuleFor(doc => doc.Author)
            .NotEmpty().WithMessage("Please enter a Document Author!")
            .MaximumLength(50).WithMessage("The task name must not exceed 50 characters!");
        
        
    }
}