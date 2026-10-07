using Prepstack.Application.Common;
using Prepstack.Domain.Auth;
using Prepstack.Domain.Common;

namespace Prepstack.Application.Auth.Commands;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IIdGenerator idGenerator,
    AuthSessionIssuer sessionIssuer,
    TimeProvider timeProvider)
{
    public async Task<Result<AuthTokens>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = User.NormalizeEmail(command.Email);
        var existing = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existing is not null)
        {
            return Error.Conflict("auth.email_taken", "An account with this email already exists.");
        }

        var now = timeProvider.GetUtcNow();
        var passwordHash = passwordHasher.Hash(command.Password);
        var user = User.Register(idGenerator.NewId(), command.Email, passwordHash, command.DisplayName, now);
        await userRepository.AddAsync(user, cancellationToken);

        var tokens = await sessionIssuer.IssueNewSessionAsync(user, now, cancellationToken);
        return Result<AuthTokens>.Success(tokens);
    }
}
