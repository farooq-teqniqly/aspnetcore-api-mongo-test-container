using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Api;
using Api.Models;
using Api.Repositories;
using Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using OneOf;
using OneOf.Types;
using LoginRequest = Api.Models.LoginRequest;

namespace ApiTests;
public class LoginTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly string _provider = "google";

    public LoginTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Can_Whitelist_Account()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new WhitelistAccountRequest(Faker.Internet.Email(), _provider);

        // Act
        var response = await client.PostAsync(
            "account/whitelist",
            new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                new MediaTypeHeaderValue("application/json")));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            var isWhitelistedResult = await accountRepository.AccountIsWhitelisted(request);

            isWhitelistedResult.AsT0.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Can_Safely_Call_Whitelist_Account_Multiple_Times_For_Same_Account()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new WhitelistAccountRequest(Faker.Lorem.GetFirstWord(), _provider);

        // Act

        foreach (var _ in Enumerable.Range(0, 2))
        {
            var response = await client.PostAsync(
                "account/whitelist",
                new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    new MediaTypeHeaderValue("application/json")));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task Login_Allowed_When_Account_Is_Whitelisted_And_Account_Is_Registered()
    {
        // Arrange
        var whitelistAccountRequest = new WhitelistAccountRequest(Faker.Internet.Email(), _provider);
        var loginRequest = new LoginRequest(Faker.RandomNumber.Next().ToString());

        var validatedToken = new ValidatedToken(whitelistAccountRequest.AccountName, whitelistAccountRequest.Provider, Faker.Lorem.GetFirstWord());


        using (var scope = _factory.Services.CreateScope())
        {
            var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            var whitelistedResult = await accountRepository.WhitelistAccountAsync(whitelistAccountRequest);

            whitelistedResult.AsT0.Should().BeOfType<Success>();
        }

        var client = _factory.WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(ConfigureTestServices))
            .CreateClient();

        // Act
        var response = await client.PostAsync(
            "account/login",
            new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                new MediaTypeHeaderValue("application/json")));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        loginResponse!.User.Should().Be(whitelistAccountRequest.AccountName);
        loginResponse.Role.Should().Be(Roles.User.ToString().ToLower());

        using (var scope = _factory.Services.CreateScope())
        {
            var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();

            var registerAccountRequest = new RegisterAccountRequest(
                whitelistAccountRequest.AccountName,
                whitelistAccountRequest.Provider,
                validatedToken.ProviderId);

            var registeredResult = await accountRepository.AccountIsRegistered(registerAccountRequest);

            registeredResult.AsT0.Should().BeTrue();

        }

        return;

        void ConfigureTestServices(IServiceCollection services)
        {
            var tokenValidatorServiceDescriptor = services.Single(d => d.ServiceType == typeof(ITokenValidatorService));

            services.Remove(tokenValidatorServiceDescriptor);

            var mockTokenValidatorService = Substitute.For<ITokenValidatorService>();

            mockTokenValidatorService.ValidateTokenAsync(loginRequest.Token)
                .Returns(Task.FromResult<OneOf<Success<ValidatedToken>, Error>>(
                    new Success<ValidatedToken>(validatedToken)));

            services.AddScoped(_ => mockTokenValidatorService);
        }
    }

    [Fact]
    public async Task Login_Not_Allowed_When_Account_Not_Whitelisted()
    {
        // Arrange
        var loginRequest = new LoginRequest(Faker.RandomNumber.Next().ToString());

        var client = _factory.WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(ConfigureTestServices))
            .CreateClient();

        // Act
        var response = await client.PostAsync(
            "account/login",
        new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                new MediaTypeHeaderValue("application/json")));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        return;

        void ConfigureTestServices(IServiceCollection services)
        {
            var tokenValidatorServiceDescriptor = services.Single(d => d.ServiceType == typeof(ITokenValidatorService));

            services.Remove(tokenValidatorServiceDescriptor);

            var mockTokenValidatorService = Substitute.For<ITokenValidatorService>();

            var validatedToken = new ValidatedToken(Faker.Internet.Email(), _provider, Faker.Lorem.GetFirstWord());

            mockTokenValidatorService.ValidateTokenAsync(Arg.Any<string>())
                .Returns(Task.FromResult<OneOf<Success<ValidatedToken>, Error>>(
                    new Success<ValidatedToken>(validatedToken)));

            services.AddScoped(_ => mockTokenValidatorService);
        }
    }
}
