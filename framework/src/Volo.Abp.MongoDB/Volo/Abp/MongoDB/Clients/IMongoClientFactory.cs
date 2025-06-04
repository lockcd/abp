using MongoDB.Driver;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.MongoDB.Clients;

public interface IMongoClientFactory : ISingletonDependency
{
    MongoClient GetClient(string connectionString);
}