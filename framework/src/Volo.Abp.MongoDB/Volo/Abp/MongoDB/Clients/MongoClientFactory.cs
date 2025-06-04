using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Volo.Abp.MongoDB.Clients;

public class MongoClientFactory : IMongoClientFactory
{
    private readonly ConcurrentDictionary<string, MongoClient> _clients = new();
    private readonly AbpMongoDbContextOptions Options;

    public MongoClientFactory(IOptions<AbpMongoDbContextOptions> options)
    {
        Options = options.Value;
    }

    public MongoClient GetClient(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string must not be null or empty.", nameof(connectionString));
        }

        return _clients.GetOrAdd(connectionString, cs =>
        {
            var mongoClientSettings = MongoClientSettings.FromUrl(new MongoUrl(cs));
            Options.MongoClientSettingsConfigurer?.Invoke(mongoClientSettings);
            return new MongoClient(mongoClientSettings);
        });
    }
}