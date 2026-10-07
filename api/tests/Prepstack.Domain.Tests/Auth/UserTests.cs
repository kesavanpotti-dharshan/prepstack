using Prepstack.Domain.Auth;
using Shouldly;

namespace Prepstack.Domain.Tests.Auth;

public class UserTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Register_NormalizesEmailToLowercaseAndTrimmed()
    {
        var user = User.Register("id-1", "  Person@Example.COM  ", "hash", "Display Name", Now);

        user.Email.ShouldBe("person@example.com");
    }

    [Fact]
    public void Register_TrimsDisplayName()
    {
        var user = User.Register("id-1", "person@example.com", "hash", "  Display Name  ", Now);

        user.DisplayName.ShouldBe("Display Name");
    }

    [Fact]
    public void Register_NewUser_HasNoRoles()
    {
        var user = User.Register("id-1", "person@example.com", "hash", "Display Name", Now);

        user.Roles.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_BlankEmail_Throws(string email)
    {
        Should.Throw<ArgumentException>(() => User.Register("id-1", email, "hash", "Display Name", Now));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_BlankDisplayName_Throws(string displayName)
    {
        Should.Throw<ArgumentException>(() => User.Register("id-1", "person@example.com", "hash", displayName, Now));
    }

    [Fact]
    public void Register_BlankPasswordHash_Throws()
    {
        Should.Throw<ArgumentException>(() => User.Register("id-1", "person@example.com", "  ", "Display Name", Now));
    }
}
