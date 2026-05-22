using FluentValidation;
using TaskTracker.Api.Dtos.Tasks;


namespace TaskTracker.Api.Validators;

public class UpdateTaskStatusRequestValidator: AbstractValidator<UpdateTaskStatusRequest>
{
    public UpdateTaskStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}