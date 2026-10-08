using FluentValidation;

namespace Prepstack.Application.Topics.Commands;

public sealed class CreateTopicCommandValidator : AbstractValidator<CreateTopicCommand>
{
    public CreateTopicCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(2_000);
        RuleFor(x => x.Visibility).Must(v => v is "private" or "public")
            .WithMessage("Visibility must be 'private' or 'public'.");
    }
}
