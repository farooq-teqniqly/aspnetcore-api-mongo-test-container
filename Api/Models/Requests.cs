using FluentValidation;

namespace Api.Models;

public record LoginRequest(string Token);

public record WhitelistAccountRequest(string AccountName, string Provider);

public class WhitelistAccountRequestValidator : AbstractValidator<WhitelistAccountRequest>
{
    public WhitelistAccountRequestValidator()
    {
        RuleFor(r => r.AccountName).NotNull().NotEmpty();
        RuleFor(r => r.Provider).NotNull().NotEmpty();
    }
}

public record RegisterAccountRequest(
    string AccountName,
    string Provider,
    string ProviderId,
    Roles Role = Roles.User);


public class RegisterAccountRequestValidator : AbstractValidator<RegisterAccountRequest>
{
    public RegisterAccountRequestValidator()
    {
        RuleFor(r => r.AccountName).NotNull().NotEmpty();
        RuleFor(r => r.Provider).NotNull().NotEmpty();
        RuleFor(r => r.ProviderId).NotNull().NotEmpty();
    }
}
