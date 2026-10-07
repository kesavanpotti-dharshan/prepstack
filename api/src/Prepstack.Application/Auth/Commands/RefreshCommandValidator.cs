using FluentValidation;

namespace Prepstack.Application.Auth.Commands;

public sealed class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator()
    {
        RuleFor(x => x.RawToken).NotEmpty();
    }
}
