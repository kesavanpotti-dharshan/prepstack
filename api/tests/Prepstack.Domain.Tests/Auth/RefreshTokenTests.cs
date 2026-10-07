using Prepstack.Domain.Auth;
using Shouldly;

namespace Prepstack.Domain.Tests.Auth;

public class RefreshTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Issue_SetsExpiresAtFromLifetime()
    {
        var token = RefreshToken.Issue("id-1", "user-1", "hash", "family-1", Now, TimeSpan.FromDays(14));

        token.ExpiresAt.ShouldBe(Now + TimeSpan.FromDays(14));
    }

    [Fact]
    public void IsActive_WhenNotRevokedAndNotExpired_ReturnsTrue()
    {
        var token = RefreshToken.Issue("id-1", "user-1", "hash", "family-1", Now, TimeSpan.FromDays(14));

        token.IsActive(Now.AddDays(1)).ShouldBeTrue();
    }

    [Fact]
    public void IsActive_AfterExpiry_ReturnsFalse()
    {
        var token = RefreshToken.Issue("id-1", "user-1", "hash", "family-1", Now, TimeSpan.FromDays(14));

        token.IsActive(Now.AddDays(15)).ShouldBeFalse();
    }

    [Fact]
    public void IsActive_AfterRevoke_ReturnsFalse()
    {
        var token = RefreshToken.Issue("id-1", "user-1", "hash", "family-1", Now, TimeSpan.FromDays(14));
        token.Revoke(Now.AddHours(1));

        token.IsActive(Now.AddHours(2)).ShouldBeFalse();
    }

    [Fact]
    public void Revoke_SetsRevokedAtAndReplacedBy()
    {
        var token = RefreshToken.Issue("id-1", "user-1", "hash", "family-1", Now, TimeSpan.FromDays(14));
        var revokedAt = Now.AddHours(1);

        token.Revoke(revokedAt, replacedBy: "id-2");

        token.RevokedAt.ShouldBe(revokedAt);
        token.ReplacedBy.ShouldBe("id-2");
    }

    [Fact]
    public void Revoke_CalledTwice_KeepsFirstRevocation()
    {
        var token = RefreshToken.Issue("id-1", "user-1", "hash", "family-1", Now, TimeSpan.FromDays(14));
        var firstRevokedAt = Now.AddHours(1);

        token.Revoke(firstRevokedAt, replacedBy: "id-2");
        token.Revoke(Now.AddHours(2), replacedBy: "id-3");

        token.RevokedAt.ShouldBe(firstRevokedAt);
        token.ReplacedBy.ShouldBe("id-2");
    }
}
