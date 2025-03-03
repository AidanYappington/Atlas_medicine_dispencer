using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;

public static class SchedulerService
{
    // Maak een Quartz scheduler
    private static readonly IScheduler _scheduler = new StdSchedulerFactory().GetScheduler().Result;
    static SchedulerService()
    {
        _scheduler.Start().Wait();
    }

    public static async Task AddJob<T>(string jobName, string groupName, int intervalInSeconds) where T : IJob
    {
        IJobDetail job = JobBuilder.Create<T>()
            .WithIdentity(jobName, groupName)
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobName}Trigger", groupName)
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(intervalInSeconds)
                .RepeatForever())
            .Build();

        await _scheduler.ScheduleJob(job, trigger);
        Console.WriteLine($"Scheduled {jobName} to run every {intervalInSeconds} seconds.");
    }
}
