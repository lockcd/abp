using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;

namespace Volo.Abp.RabbitMQ;

public class ConnectionPool : IConnectionPool, ISingletonDependency
{
    protected AbpRabbitMqOptions Options { get; }

    protected ConcurrentDictionary<string, IConnection> Connections { get; }

    protected SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

    private bool _isDisposed;

    public ConnectionPool(IOptions<AbpRabbitMqOptions> options)
    {
        Options = options.Value;
        Connections = new ConcurrentDictionary<string, IConnection>();
    }

    public virtual async Task<IConnection> GetAsync(string? connectionName = null)
    {
        connectionName ??= RabbitMqConnections.DefaultConnectionName;

        IConnection connection;

        if (Connections.TryGetValue(connectionName, out var existingConnection))
        {
            connection = existingConnection;
        }
        else
        {
            using (await Semaphore.LockAsync())
            {
                try
                {
                    var connectionFactory = Options.Connections.GetOrDefault(connectionName);
                    if (Connections.TryGetValue(connectionName, out var existingConnection2))
                    {
                        connection = existingConnection2;
                    }
                    else
                    {
                        connection = await GetConnectionAsync(connectionName, connectionFactory);
                        Connections.TryAdd(connectionName, connection);

                        if (!connection.IsOpen)
                        {
                            connection.Dispose();
                            Connections.TryRemove(connectionName, out _);
                            connection = await GetConnectionAsync(connectionName, connectionFactory);
                            Connections.TryAdd(connectionName, connection);
                        }
                    }
                }
                catch (Exception)
                {
                    Connections.TryRemove(connectionName, out _);
                    throw;
                }
            }
        }

        return connection;
    }

    protected virtual async Task<IConnection> GetConnectionAsync(string connectionName, ConnectionFactory connectionFactory)
    {
        var hostnames = connectionFactory.HostName.TrimEnd(';').Split(';');
        // Handle Rabbit MQ Cluster.
        return hostnames.Length == 1
            ? await connectionFactory.CreateConnectionAsync()
            : await connectionFactory.CreateConnectionAsync(hostnames);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        foreach (var connection in Connections.Values)
        {
            try
            {
                connection.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        Connections.Clear();
    }
}
