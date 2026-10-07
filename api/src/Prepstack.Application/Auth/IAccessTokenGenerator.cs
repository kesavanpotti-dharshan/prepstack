using Prepstack.Domain.Auth;

namespace Prepstack.Application.Auth;

public interface IAccessTokenGenerator
{
    AccessToken Generate(User user, DateTimeOffset now);
}

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
