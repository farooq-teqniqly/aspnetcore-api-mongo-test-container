namespace Api;

public record AccountDto(
    string Id,
    string AccountName,
    string Provider,
    string ProviderId,
    string Role);

public record WhitelistedAccountDto(
    string Id,
    string AccountName,
    string Provider);
