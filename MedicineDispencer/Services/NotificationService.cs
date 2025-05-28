using System.Timers;

public class NotificationService
{
    private readonly CompartmentsData _compartmentsData;
    private readonly System.Timers.Timer _timer;
    private bool notify = false;
    private List<(int, MedicijnCompartiment)> dueCompartments = [];
    public Action<List<(int, MedicijnCompartiment)>>? OnNotification;


    private DateTime? lastDismissedAt = null;

    public NotificationService(CompartmentsData compartmentsData)
    {
        _compartmentsData = compartmentsData;
        _timer = new System.Timers.Timer(60_000); // Check every minute
        _timer.Elapsed += CheckDosingTimes;
        _timer.Start();
    }

    public void TriggerManualReminder(int index, MedicijnCompartiment compartment)
    {
        dueCompartments.Add((index +1, compartment));
        OnNotification?.Invoke(dueCompartments);
    }
    private void CheckDosingTimes(object? sender, ElapsedEventArgs e)
    {
        var now = DateTime.Now;

        for (int i = 0; i < _compartmentsData.compartments.Length; i++)
        {
            if (_compartmentsData.compartments[i] == null) continue;
            foreach (var tijd in _compartmentsData.compartments[i].DoseringstijdenPerDag)
            {
                // if (Math.Abs((now.TimeOfDay - tijd).TotalMinutes) < 1)
                // {
                dueCompartments.Add((i + 1, _compartmentsData.compartments[i]));
                notify = true;
                break;
                // }
            }
        }

        if (!notify && lastDismissedAt.HasValue && (now - lastDismissedAt.Value).TotalMinutes > 5)
        {
            notify = true;
        }

        if (notify)
        {
            LEDService.TurnOn();
            notify = false;
            OnNotification?.Invoke(dueCompartments);
            lastDismissedAt = null;
            return;
        }
    }

    // Call this when the user dismisses a reminder
    public void DismissReminder()
    {
        LEDService.TurnOff();
        lastDismissedAt = DateTime.Now;
    }

    // Call this when medication is taken to clear the dismissal
    public void ClearReminders()
    {
        LEDService.TurnOff();
        lastDismissedAt = null;
        dueCompartments.Clear();
    }
}