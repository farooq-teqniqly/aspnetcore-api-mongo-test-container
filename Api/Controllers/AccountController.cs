using Api.Models;
using Api.Repositories;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITokenValidatorService _tokenValidatorService;

    public AccountController(
        IAccountRepository accountRepository,
        ITokenValidatorService tokenValidatorService)
    {
        _accountRepository = accountRepository;
        _tokenValidatorService = tokenValidatorService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        try
        {
            var tokenValidationResult = await _tokenValidatorService.ValidateTokenAsync(loginRequest.Token);

            if (tokenValidationResult.IsT1)
            {
                return Forbid();
            }

            var validatedToken = tokenValidationResult.AsT0.Value;

            var whitelistAccountRequest = new WhitelistAccountRequest(
                validatedToken.AccountName,
                validatedToken.Provider);

            var accountIsWhitelistedResult = await _accountRepository.AccountIsWhitelisted(
                whitelistAccountRequest);

            if (accountIsWhitelistedResult.IsT1)
            {
                return StatusCode(500);
            }

            var whitelisted = accountIsWhitelistedResult.AsT0;

            if (!whitelisted)
            {
                return Forbid();
            }

            var registerAccountRequest = new RegisterAccountRequest(
                validatedToken.AccountName,
                validatedToken.Provider,
                validatedToken.ProviderId);

            var registerAccountResult = await _accountRepository.RegisterAccountAsync(
                registerAccountRequest);

            if (registerAccountResult.IsT1)
            {
                return StatusCode(500);
            }

            return Ok(
                new LoginResponse(
                    validatedToken.AccountName,
                    registerAccountRequest.Role.ToString().ToLower()));
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

}
