using Api.Models;
using Api.MongoDb;
using FluentValidation;
using MongoDB.Bson;
using MongoDB.Driver;
using OneOf;
using OneOf.Types;

namespace Api.Repositories;

public interface IAccountRepository
{
    Task<OneOf<Success, Error>> WhitelistAccountAsync(WhitelistAccountRequest request);
    Task<OneOf<Success, Error>> RegisterAccountAsync(RegisterAccountRequest request);

    Task<OneOf<bool, Error>> AccountIsWhitelisted(WhitelistAccountRequest request);

    Task<OneOf<bool, Error>> AccountIsRegistered(RegisterAccountRequest request);
}

public class AccountRepository : IAccountRepository
{
    private readonly IMongoDatabase _db;
    private readonly IValidator<WhitelistAccountRequest> _whitelistAccountRequestValidator;
    private readonly IValidator<RegisterAccountRequest> _registerAccountRequestValidator;

    public AccountRepository(
        IMongoDatabase db,
        IValidator<WhitelistAccountRequest> whitelistAccountRequestValidator,
        IValidator<RegisterAccountRequest> registerAccountRequestValidator,
        ILogger<AccountRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(logger);

        _db = db;
        _whitelistAccountRequestValidator = whitelistAccountRequestValidator;
        _registerAccountRequestValidator = registerAccountRequestValidator;
    }

    public async Task<OneOf<Success, Error>> WhitelistAccountAsync(WhitelistAccountRequest request)
    {
        var valid = (await _whitelistAccountRequestValidator.ValidateAsync(request).ConfigureAwait(false)).IsValid;

        if (!valid)
        {
            return new Error();
        }

        var accountIsWhitelistedResult = await AccountIsWhitelisted(request);

        if (accountIsWhitelistedResult.IsT0)
        {
            if (accountIsWhitelistedResult.AsT0)
            {
                return new Success();
            }
        }
        else
        {
            return new Error();
        }

        var dto = new WhitelistedAccountDto(
            ObjectId.GenerateNewId().ToString(),
            request.AccountName,
            request.Provider);

        await GetWhitelistedAccountCollectionAs<WhitelistedAccountDto>().InsertOneAsync(dto).ConfigureAwait(false);

        return new Success();
    }

    public async Task<OneOf<Success, Error>> RegisterAccountAsync(RegisterAccountRequest request)
    {
        var valid = (await _registerAccountRequestValidator.ValidateAsync(request).ConfigureAwait(false)).IsValid;

        if (!valid)
        {
            return new Error();
        }

        var accountIsRegisteredResult = await AccountIsRegistered(request);

        if (accountIsRegisteredResult.IsT0)
        {
            if (accountIsRegisteredResult.AsT0)
            {
                return new Success();
            }
        }
        else
        {
            return new Error();
        }

        var dto = new AccountDto(
            ObjectId.GenerateNewId().ToString(),
            request.AccountName,
            request.Provider,
            request.ProviderId,
            request.Role.ToString().ToLower());

        await GetAccountCollectionAs<AccountDto>().InsertOneAsync(dto).ConfigureAwait(false);

        return new Success();
    }

    public async Task<OneOf<bool, Error>> AccountIsWhitelisted(WhitelistAccountRequest request)
    {
        var valid = (await _whitelistAccountRequestValidator.ValidateAsync(request).ConfigureAwait(false)).IsValid;

        if (!valid)
        {
            return new Error();
        }

        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("accountName", request.AccountName),
            Builders<BsonDocument>.Filter.Eq("provider", request.Provider));

        var document = await GetWhitelistedAccountCollectionAs<BsonDocument>()
            .Find(filter)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        return document != null;
    }

    public async Task<OneOf<bool, Error>> AccountIsRegistered(RegisterAccountRequest request)
    {
        var valid = (await _registerAccountRequestValidator.ValidateAsync(request).ConfigureAwait(false)).IsValid;

        if (!valid)
        {
            return new Error();
        }

        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("accountName", request.AccountName),
            Builders<BsonDocument>.Filter.Eq("provider", request.Provider));

        var document = await GetAccountCollectionAs<BsonDocument>()
            .Find(filter)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        return document != null;
    }

    private IMongoCollection<T> GetWhitelistedAccountCollectionAs<T>() => _db.GetCollection<T>(WhitelistedAccountMapping.CollectionName);

    private IMongoCollection<T> GetAccountCollectionAs<T>() => _db.GetCollection<T>(AccountMapping.CollectionName);
}
