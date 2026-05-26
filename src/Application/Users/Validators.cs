using ApiBozzis.Application.Common;
using FluentValidation;

namespace ApiBozzis.Application.Users;

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(120);
        RuleFor(x => x.PhotoUrl).MaximumLength(2048);
        RuleFor(x => x.BirthDate)
            .Must(d => d is null || (d.Value.Year >= 1900 && d.Value <= DateOnly.FromDateTime(DateTime.UtcNow)))
            .WithMessage("Invalid birth date.");
    }
}

public sealed class SetDocumentRequestValidator : AbstractValidator<SetDocumentRequest>
{
    public SetDocumentRequestValidator()
    {
        RuleFor(x => x.Document)
            .NotEmpty()
            .Must((req, doc) => DocumentValidator.IsValid(doc, req.DocumentType))
            .WithMessage("Invalid document for the specified type.");
    }
}
