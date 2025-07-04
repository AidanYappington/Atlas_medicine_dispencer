using System.Timers;

namespace MedicineDispencer.Services;
public class NotificationService
{
    private readonly System.Timers.Timer _timer;
    private bool notify = false;
    public List<(int, MedicijnCompartiment)> dueCompartments = [];
    public bool IsNotificationActive = false;
    public event Action? OnNotification;
    public event Action? OnMedicationTaken;

    private DateTime? lastDismissedAt = null;

    public NotificationService()
    {
        _timer = new System.Timers.Timer(60_000); // Check every minute
        _timer.Elapsed += CheckDosingTimes;
        _timer.Start();
    }

    public void TriggerManualReminder(int index, MedicijnCompartiment compartment)
    {
        IsNotificationActive = true;
        dueCompartments.Add((index + 1, compartment));
        ButtonService.StartPolling();
        LEDService.TurnOn();
        OnNotification?.Invoke();

        // Open servo, then close after 1 second
        ServoService.Dispense();
    }
    private async void CheckDosingTimes(object? sender, ElapsedEventArgs e)
    {
        var now = DateTime.Now;

        for (int i = 0; i < CompartmentsData.compartments.Length; i++)
        {
            if (CompartmentsData.compartments[i] == null) continue;
            foreach (var tijd in CompartmentsData.compartments[i].DoseringstijdenPerDag)
            {
                if (Math.Abs((now.TimeOfDay - tijd).TotalMinutes) < 1)
                {
                    dueCompartments.Add((i + 1, CompartmentsData.compartments[i]));
                    notify = true;
                    await ServoService.Dispense(); // Dispense medication asynchronously
                    break;
                }
            }
        }

        if (!notify && lastDismissedAt.HasValue && (now - lastDismissedAt.Value).TotalMinutes > 5)
        {
            notify = true;
        }

        if (notify)
        {
            ButtonService.StartPolling();
            LEDService.TurnOn();
            notify = false;
            IsNotificationActive = true;
            OnNotification?.Invoke();
            lastDismissedAt = null;
            return;
        }
    }

    // Call this when the user dismisses a reminder
    public void DismissReminder()
    {
        ButtonService.StopPolling();
        LEDService.TurnOff();
        IsNotificationActive = false;
        lastDismissedAt = DateTime.Now;
        OnNotification?.Invoke();
    }

    // Call this when medication is taken to clear the dismissal
    public void ClearReminders()
    {
        ButtonService.StopPolling();
        LEDService.TurnOff();
        lastDismissedAt = null;
        IsNotificationActive = false;
        dueCompartments.Clear();
        OnNotification?.Invoke();
    }

    public void MedicationTaken()
    {
        OnMedicationTaken?.Invoke();
    }
}