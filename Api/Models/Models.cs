namespace Api.Models;

public record ValidatedToken(
    string AccountName,
    string Provider,
    string ProviderId);

public enum Roles
{
    User,
    Admin
}
