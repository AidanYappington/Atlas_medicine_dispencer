using Quartz;

public class DispenseJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine($"Job executed at {DateTime.Now}");
        return Task.CompletedTask;
    }
}