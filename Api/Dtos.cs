using MongoDB.Bson;

namespace Api;

public record AccountDto
{
    public string Id { get; }
    public string AccountName { get; }
    public string Provider { get; }
    public string ProviderId { get; }
    public string Role { get; }

    public AccountDto(
        string accountName,
        string provider,
        string providerId,
        string role)
    {
        Id = ObjectId.GenerateNewId().ToString();
        AccountName = accountName;
        Provider = provider;
        ProviderId = providerId;
        Role = role;
    }
}

public record WhitelistedAccountDto
{
    public string Id { get; }
    public string AccountName { get; }
    public string Provider { get; }

    public WhitelistedAccountDto(
        string accountName,
        string provider)
    {
        Id = ObjectId.GenerateNewId().ToString();
        AccountName = accountName;
        Provider = provider;
    }
}

