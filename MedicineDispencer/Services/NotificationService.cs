using System.Timers;

public class NotificationService : IDisposable
{
    private readonly CompartmentsData _compartmentsData;
    private readonly System.Timers.Timer _timer;
    public event Action<string>? OnNotification;

    public NotificationService(CompartmentsData compartmentsData)
    {
        _compartmentsData = compartmentsData;
        _timer = new System.Timers.Timer(50_000); // Check every minute
        _timer.Elapsed += CheckDosingTimes;
        _timer.Start();
    }

    private void CheckDosingTimes(object? sender, ElapsedEventArgs e)
    {
        var now = DateTime.Now;
        foreach (var comp in _compartmentsData.compartments)
        {
            if (comp == null) continue;
            foreach (var tijd in comp.DoseringstijdenPerDag)
            {
                if (Math.Abs((now.TimeOfDay - tijd).TotalMinutes) < 1)
                {
                    OnNotification?.Invoke($"Tijd voor {comp.MedicijnNaam} in compartiment!");
                }
            }
        }
    }

    public void Dispose() => _timer.Dispose();
}