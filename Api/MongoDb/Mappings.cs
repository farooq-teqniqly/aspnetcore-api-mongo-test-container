using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Api.MongoDb;

public class AccountMapping
{
    public static readonly string CollectionName = "accounts";

    public static void Configure(IMongoDatabase db)
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(AccountDto)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<AccountDto>(map =>
        {
            map.AutoMap();

            map.MapIdMember(c => c.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));
            map.MapMember(c => c.AccountName).SetElementName("accountName");
            map.MapMember(c => c.Provider).SetElementName("provider");
            map.MapMember(c => c.ProviderId).SetElementName("providerId");
            map.MapMember(c => c.Role).SetElementName("role");
        });

        var collection = db.GetCollection<AccountDto>(CollectionName);

        var indexKeysDefinition = Builders<AccountDto>.IndexKeys.Ascending(a => a.AccountName);

        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<AccountDto>(indexKeysDefinition, indexOptions);

        collection.Indexes.CreateOne(indexModel);

    }
}

public class WhitelistedAccountMapping
{
    public static readonly string CollectionName = "whitelistedAccounts";

    public static void Configure(IMongoDatabase db)
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(WhitelistedAccountDto)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<WhitelistedAccountDto>(map =>
        {
            map.AutoMap();

            map.MapIdMember(c => c.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));
            map.MapMember(c => c.AccountName).SetElementName("accountName");
            map.MapMember(c => c.Provider).SetElementName("provider");
        });

        var collection = db.GetCollection<WhitelistedAccountDto>(CollectionName);

        var indexKeysDefinition = Builders<WhitelistedAccountDto>.IndexKeys.Ascending(a => a.AccountName);

        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<WhitelistedAccountDto>(indexKeysDefinition, indexOptions);

        collection.Indexes.CreateOne(indexModel);

    }

}

public static class MongoMappings
{
    private static readonly object _lock = new();

    public static void RegisterMappings(IMongoDatabase db)
    {
        lock (_lock)
        {
            AccountMapping.Configure(db);
            WhitelistedAccountMapping.Configure(db);
        }
    }
}
