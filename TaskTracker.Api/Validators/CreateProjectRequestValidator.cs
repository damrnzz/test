using FluentValidation;
using TaskTracker.Api.Dtos.Projects;

namespace TaskTracker.Api.Validators;

public class CreateProjectRequestValidator: AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(20)
            .Matches("^[A-Z0-9_-]+$");
    }
}