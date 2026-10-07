using Prepstack.Domain.Auth;
using Prepstack.Domain.Common;

namespace Prepstack.Application.Auth.Commands;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    AuthSessionIssuer sessionIssuer,
    TimeProvider timeProvider)
{
    public async Task<Result<AuthTokens>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = User.NormalizeEmail(command.Email);
        var user = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null || !passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            return Error.Unauthorized("auth.invalid_credentials", "Invalid email or password.");
        }

        var now = timeProvider.GetUtcNow();
        var tokens = await sessionIssuer.IssueNewSessionAsync(user, now, cancellationToken);
        return Result<AuthTokens>.Success(tokens);
    }
}
