namespace Prepstack.Application.Auth.Commands;

public sealed record RegisterCommand(string Email, string Password, string DisplayName);
