using FluentValidation;
using TaskTracker.Api.Dtos.Attachments;

namespace TaskTracker.Api.Validators;

public class CreateAttachmentRequestValidator : AbstractValidator<CreateAttachmentRequest>
{
    public CreateAttachmentRequestValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.SizeBytes)
            .GreaterThan(0);
    }
}