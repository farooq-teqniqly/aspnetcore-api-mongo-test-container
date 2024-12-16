using Api.Models;
using OneOf;
using OneOf.Types;

namespace Api.Services;

public interface ITokenValidatorService
{
    Task<OneOf<Success<ValidatedToken>, Error>> ValidateTokenAsync(string token);
}
public class TokenValidatorService : ITokenValidatorService
{
    public Task<OneOf<Success<ValidatedToken>, Error>> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult<OneOf<Success<ValidatedToken>, Error>>(new Error());
        }

        return Task.FromResult<OneOf<Success<ValidatedToken>, Error>>(new Success<ValidatedToken>(new ValidatedToken("foo@bar.com", "google", "1234")));

    }
}
