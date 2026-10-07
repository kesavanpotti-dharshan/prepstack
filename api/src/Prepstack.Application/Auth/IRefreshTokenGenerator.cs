namespace Prepstack.Application.Auth;

public interface IRefreshTokenGenerator
{
    RefreshTokenSecret Generate();

    string Hash(string rawToken);
}

public sealed record RefreshTokenSecret(string RawToken, string Hash);
