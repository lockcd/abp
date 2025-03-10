﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Timing;

namespace Volo.Abp.BackgroundJobs;

/// <summary>
/// Default implementation of <see cref="IBackgroundJobManager"/>.
/// </summary>
[Dependency(ReplaceServices = true)]
public class DefaultBackgroundJobManager : IBackgroundJobManager, ITransientDependency
{
    protected IClock Clock { get; }
    protected IBackgroundJobSerializer Serializer { get; }
    protected IGuidGenerator GuidGenerator { get; }
    protected IBackgroundJobStore Store { get; }
    protected IOptions<AbpBackgroundJobWorkerOptions> BackgroundJobWorkerOptions { get; }

    public DefaultBackgroundJobManager(
        IClock clock,
        IBackgroundJobSerializer serializer,
        IBackgroundJobStore store,
        IGuidGenerator guidGenerator,
        IOptions<AbpBackgroundJobWorkerOptions> backgroundJobWorkerOptions)
    {
        Clock = clock;
        Serializer = serializer;
        GuidGenerator = guidGenerator;
        BackgroundJobWorkerOptions = backgroundJobWorkerOptions;
        Store = store;
    }

    public virtual async Task<string> EnqueueAsync<TArgs>(TArgs args, BackgroundJobPriority priority = BackgroundJobPriority.Normal, TimeSpan? delay = null)
    {
        var jobName = BackgroundJobNameAttribute.GetName<TArgs>();
        var jobId = await EnqueueAsync(jobName, args!, priority, delay);
        return jobId.ToString();
    }

    protected virtual async Task<Guid> EnqueueAsync(string jobName, object args, BackgroundJobPriority priority = BackgroundJobPriority.Normal, TimeSpan? delay = null)
    {
        var jobInfo = new BackgroundJobInfo
        {
            Id = GuidGenerator.Create(),
            ApplicationName = BackgroundJobWorkerOptions.Value.ApplicationName,
            JobName = jobName,
            JobArgs = Serializer.Serialize(args),
            Priority = priority,
            CreationTime = Clock.Now,
            NextTryTime = Clock.Now
        };

        if (delay.HasValue)
        {
            jobInfo.NextTryTime = Clock.Now.Add(delay.Value);
        }

        await Store.InsertAsync(jobInfo);

        return jobInfo.Id;
    }
}
