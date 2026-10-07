using NSubstitute;
using Prepstack.Application.Auth;
using Prepstack.Application.Auth.Commands;
using Prepstack.Application.Common;
using Prepstack.Domain.Auth;
using Prepstack.Domain.Common;
using Shouldly;

namespace Prepstack.Application.Tests.Auth.Commands;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IIdGenerator _idGenerator = Substitute.For<IIdGenerator>();
    private readonly IAccessTokenGenerator _accessTokenGenerator = Substitute.For<IAccessTokenGenerator>();
    private readonly IRefreshTokenGenerator _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();

    private LoginCommandHandler CreateHandler()
    {
        var sessionIssuer = new AuthSessionIssuer(
            _accessTokenGenerator,
            _refreshTokenGenerator,
            _refreshTokenRepository,
            _idGenerator);

        return new LoginCommandHandler(_userRepository, _passwordHasher, sessionIssuer, TimeProvider.System);
    }

    [Fact]
    public async Task Handle_UnknownEmail_ReturnsUnauthorized()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await CreateHandler().Handle(
            new LoginCommand("nobody@example.com", "password123"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsUnauthorized()
    {
        var user = User.Register("id-1", "person@example.com", "hashed", "Display", DateTimeOffset.UtcNow);
        _userRepository.GetByEmailAsync("person@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("wrong-password", "hashed").Returns(false);

        var result = await CreateHandler().Handle(
            new LoginCommand("person@example.com", "wrong-password"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_CorrectCredentials_IssuesTokens()
    {
        var user = User.Register("id-1", "person@example.com", "hashed", "Display", DateTimeOffset.UtcNow);
        _userRepository.GetByEmailAsync("person@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("password123", "hashed").Returns(true);
        _idGenerator.NewId().Returns("family-1", "token-1");
        _accessTokenGenerator
            .Generate(Arg.Any<User>(), Arg.Any<DateTimeOffset>())
            .Returns(call => new AccessToken("access-token", ((DateTimeOffset)call[1]).AddMinutes(15)));
        _refreshTokenGenerator.Generate().Returns(new RefreshTokenSecret("raw-refresh-token", "hashed-refresh-token"));

        var result = await CreateHandler().Handle(
            new LoginCommand("person@example.com", "password123"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.RefreshToken.ShouldBe("raw-refresh-token");
    }
}
