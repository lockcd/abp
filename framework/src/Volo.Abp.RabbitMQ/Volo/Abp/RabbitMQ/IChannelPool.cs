using System;
using System.Threading.Tasks;

namespace Volo.Abp.RabbitMQ;

public interface IChannelPool : IDisposable
{
    Task<IChannelAccessor> AcquireAsync(string? channelName = null, string? connectionName = null);
}
