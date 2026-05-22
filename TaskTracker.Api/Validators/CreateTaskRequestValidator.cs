using FluentValidation;
using TaskTracker.Api.Dtos.Tasks;

namespace TaskTracker.Api.Validators;

public class CreateTaskRequestValidator: AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.ProjectId)
            .GreaterThan(0);

        RuleFor(x => x.DueDateUtc)
            .Must(x => x == null || x > DateTime.UtcNow)
            .WithMessage("Due date must be in the future.");
    }
}