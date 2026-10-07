using NSubstitute;
using Prepstack.Application.Auth;
using Prepstack.Application.Auth.Commands;
using Prepstack.Application.Common;
using Prepstack.Domain.Auth;
using Prepstack.Domain.Common;
using Shouldly;

namespace Prepstack.Application.Tests.Auth.Commands;

public class RegisterCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IIdGenerator _idGenerator = Substitute.For<IIdGenerator>();
    private readonly IAccessTokenGenerator _accessTokenGenerator = Substitute.For<IAccessTokenGenerator>();
    private readonly IRefreshTokenGenerator _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();

    private RegisterCommandHandler CreateHandler()
    {
        var sessionIssuer = new AuthSessionIssuer(
            _accessTokenGenerator,
            _refreshTokenGenerator,
            _refreshTokenRepository,
            _idGenerator);

        return new RegisterCommandHandler(_userRepository, _passwordHasher, _idGenerator, sessionIssuer, TimeProvider.System);
    }

    [Fact]
    public async Task Handle_EmailAlreadyRegistered_ReturnsConflict()
    {
        var existing = User.Register("id-1", "person@example.com", "hash", "Display", DateTimeOffset.UtcNow);
        _userRepository.GetByEmailAsync("person@example.com", Arg.Any<CancellationToken>()).Returns(existing);

        var result = await CreateHandler().Handle(
            new RegisterCommand("person@example.com", "password123", "Display"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Type.ShouldBe(ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_NewEmail_CreatesUserAndIssuesTokens()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);
        _idGenerator.NewId().Returns("id-1", "family-1", "token-1");
        _passwordHasher.Hash("password123").Returns("hashed-password");
        _accessTokenGenerator
            .Generate(Arg.Any<User>(), Arg.Any<DateTimeOffset>())
            .Returns(call => new AccessToken("access-token", ((DateTimeOffset)call[1]).AddMinutes(15)));
        _refreshTokenGenerator.Generate().Returns(new RefreshTokenSecret("raw-refresh-token", "hashed-refresh-token"));

        var result = await CreateHandler().Handle(
            new RegisterCommand("Person@Example.com", "password123", "Display Name"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.AccessToken.ShouldBe("access-token");
        result.Value.RefreshToken.ShouldBe("raw-refresh-token");
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.Email == "person@example.com"),
            Arg.Any<CancellationToken>());
        await _refreshTokenRepository.Received(1).AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }
}
