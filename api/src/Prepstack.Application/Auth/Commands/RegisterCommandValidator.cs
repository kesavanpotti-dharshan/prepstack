using FluentValidation;

namespace Prepstack.Application.Auth.Commands;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(72);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
    }
}
