using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Volo.Abp.TestApp.Testing;
using Xunit;

namespace Volo.Abp.MongoDB.Clients;

[Collection(MongoTestCollection.Name)]
public class MongoClient_Factory_Tests : MongoDbTestBase
{
    private readonly IMongoClientFactory _factory;

    public MongoClient_Factory_Tests()
    {
        _factory = GetRequiredService<IMongoClientFactory>();
    }

    [Fact]
    public void Should_Return_Same_Instance_For_Same_ConnectionString()
    {
        // Arrange
        var connectionString = "mongodb://localhost:27017/my-db";

        // Act
        var client1 = _factory.GetClient(connectionString);
        var client2 = _factory.GetClient(connectionString);

        // Assert
        Assert.Same(client1, client2);
    }

    [Fact]
    public void Should_Return_Different_Instances_For_Different_ConnectionStrings()
    {
        // Arrange
        var cs1 = "mongodb://localhost:27017/db1";
        var cs2 = "mongodb://localhost:27017/db2";

        // Act
        var client1 = _factory.GetClient(cs1);
        var client2 = _factory.GetClient(cs2);

        // Assert
        Assert.NotSame(client1, client2);
    }

    [Fact]
    public void Should_Not_Throw_For_Valid_But_Unreachable_Connection()
    {
        // Arrange
        var cs = "mongodb://unreachablehost:12345/any";

        // Act
        var client = _factory.GetClient(cs);

        // Assert
        Assert.NotNull(client); // Even though it's not connectable now, the instance can be created
    }

    [Fact]
    public void Should_Be_ThreadSafe_When_Accessed_Concurrently()
    {
        var connectionString = "mongodb://localhost:27017/threadsafe";
        MongoClient[] results = new MongoClient[100];

        Parallel.For(0, 100, i =>
        {
            results[i] = _factory.GetClient(connectionString);
        });

        Assert.All(results, client => Assert.Same(results[0], client));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Should_Throw_If_ConnectionString_Is_Null_Or_Empty(string connectionString)
    {
        Assert.Throws<ArgumentException>(() => _factory.GetClient(connectionString));
    }
}