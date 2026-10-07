using NSubstitute;
using Prepstack.Application.Auth;
using Prepstack.Application.Auth.Commands;
using Prepstack.Domain.Auth;
using Shouldly;

namespace Prepstack.Application.Tests.Auth.Commands;

public class LogoutCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IRefreshTokenGenerator _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();

    private LogoutCommandHandler CreateHandler() =>
        new(_refreshTokenRepository, _refreshTokenGenerator, TimeProvider.System);

    [Fact]
    public async Task Handle_NoToken_ReturnsSuccessWithoutTouchingRepository()
    {
        var result = await CreateHandler().Handle(new LogoutCommand(null), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _refreshTokenRepository.DidNotReceive().RevokeAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownToken_ReturnsSuccessIdempotently()
    {
        _refreshTokenGenerator.Hash("raw").Returns("hash");
        _refreshTokenRepository.GetByTokenHashAsync("hash", Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);

        var result = await CreateHandler().Handle(new LogoutCommand("raw"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ActiveToken_RevokesIt()
    {
        var now = DateTimeOffset.UtcNow;
        var token = RefreshToken.Issue("token-1", "user-1", "hash", "family-1", now, TimeSpan.FromDays(14));
        _refreshTokenGenerator.Hash("raw").Returns("hash");
        _refreshTokenRepository.GetByTokenHashAsync("hash", Arg.Any<CancellationToken>()).Returns(token);

        var result = await CreateHandler().Handle(new LogoutCommand("raw"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _refreshTokenRepository.Received(1).RevokeAsync(
            Arg.Is<RefreshToken>(t => t.Id == "token-1" && t.RevokedAt != null),
            Arg.Any<CancellationToken>());
    }
}
