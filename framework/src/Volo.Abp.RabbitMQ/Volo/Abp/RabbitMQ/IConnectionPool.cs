using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Volo.Abp.RabbitMQ;

public interface IConnectionPool : IDisposable
{
    Task<IConnection> GetAsync(string? connectionName = null);
}
