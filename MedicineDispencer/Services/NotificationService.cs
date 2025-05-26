using System.Timers;

public class NotificationService : IDisposable
{
    private readonly CompartmentsData _compartmentsData;
    private readonly System.Timers.Timer _timer;
    public event Action<int, MedicijnCompartiment>? OnNotification;

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
        for (int i = 0; i < _compartmentsData.compartments.Length; i++)
        {
            if (_compartmentsData.compartments[i] == null) continue;
            foreach (var tijd in _compartmentsData.compartments[i].DoseringstijdenPerDag)
            {
                if (Math.Abs((now.TimeOfDay - tijd).TotalMinutes) < 1)
                {
                    OnNotification?.Invoke(i, _compartmentsData.compartments[i]);
                    break;
                }
            }
        }
    }

    public void Dispose() => _timer.Dispose();
}