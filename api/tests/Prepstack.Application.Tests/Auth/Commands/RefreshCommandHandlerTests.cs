using Microsoft.Extensions.Logging;
using NSubstitute;
using Prepstack.Application.Auth;
using Prepstack.Application.Auth.Commands;
using Prepstack.Application.Common;
using Prepstack.Domain.Auth;
using Prepstack.Domain.Common;
using Shouldly;

namespace Prepstack.Application.Tests.Auth.Commands;

public class RefreshCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IRefreshTokenGenerator _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
    private readonly IAccessTokenGenerator _accessTokenGenerator = Substitute.For<IAccessTokenGenerator>();
    private readonly IIdGenerator _idGenerator = Substitute.For<IIdGenerator>();
    private readonly ILogger<RefreshCommandHandler> _logger = Substitute.For<ILogger<RefreshCommandHandler>>();

    private RefreshCommandHandler CreateHandler() => new(
        _userRepository,
        _refreshTokenRepository,
        _refreshTokenGenerator,
        _accessTokenGenerator,
        _idGenerator,
        TimeProvider.System,
        _logger);

    [Fact]
    public async Task Handle_UnknownToken_ReturnsUnauthorized()
    {
        _refreshTokenGenerator.Hash("raw").Returns("hash");
        _refreshTokenRepository.GetByTokenHashAsync("hash", Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);

        var result = await CreateHandler().Handle(new RefreshCommand("raw"), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_AlreadyRevokedToken_RevokesWholeFamilyAndReturnsUnauthorized()
    {
        var now = DateTimeOffset.UtcNow;
        var token = RefreshToken.Issue("token-1", "user-1", "hash", "family-1", now.AddMinutes(-5), TimeSpan.FromDays(14));
        token.Revoke(now.AddMinutes(-1));
        _refreshTokenGenerator.Hash("raw").Returns("hash");
        _refreshTokenRepository.GetByTokenHashAsync("hash", Arg.Any<CancellationToken>()).Returns(token);

        var result = await CreateHandler().Handle(new RefreshCommand("raw"), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe("auth.refresh_token_reused");
        await _refreshTokenRepository.Received(1)
            .RevokeFamilyAsync("family-1", Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsUnauthorizedWithoutRevokingFamily()
    {
        var now = DateTimeOffset.UtcNow;
        var token = RefreshToken.Issue("token-1", "user-1", "hash", "family-1", now.AddDays(-15), TimeSpan.FromDays(14));
        _refreshTokenGenerator.Hash("raw").Returns("hash");
        _refreshTokenRepository.GetByTokenHashAsync("hash", Arg.Any<CancellationToken>()).Returns(token);

        var result = await CreateHandler().Handle(new RefreshCommand("raw"), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe("auth.refresh_token_expired");
        await _refreshTokenRepository.DidNotReceive()
            .RevokeFamilyAsync(Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ActiveToken_RotatesAndReturnsNewTokens()
    {
        var now = DateTimeOffset.UtcNow;
        var token = RefreshToken.Issue("token-1", "user-1", "hash", "family-1", now, TimeSpan.FromDays(14));
        var user = User.Register("user-1", "person@example.com", "hashed", "Display", now);

        _refreshTokenGenerator.Hash("raw").Returns("hash");
        _refreshTokenRepository.GetByTokenHashAsync("hash", Arg.Any<CancellationToken>()).Returns(token);
        _userRepository.GetByIdAsync("user-1", Arg.Any<CancellationToken>()).Returns(user);
        _idGenerator.NewId().Returns("token-2");
        _refreshTokenGenerator.Generate().Returns(new RefreshTokenSecret("new-raw-token", "new-hash"));
        _accessTokenGenerator
            .Generate(Arg.Any<User>(), Arg.Any<DateTimeOffset>())
            .Returns(call => new AccessToken("new-access-token", ((DateTimeOffset)call[1]).AddMinutes(15)));

        var result = await CreateHandler().Handle(new RefreshCommand("raw"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.RefreshToken.ShouldBe("new-raw-token");
        await _refreshTokenRepository.Received(1).RotateAsync(
            Arg.Is<RefreshToken>(t => t.Id == "token-1" && t.RevokedAt != null),
            Arg.Is<RefreshToken>(t => t.Id == "token-2" && t.FamilyId == "family-1"),
            Arg.Any<CancellationToken>());
    }
}
