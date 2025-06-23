using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class NotificationServiceTests
{
    [Fact]
    public void TriggerManualReminder_ActivatesNotification_AndAddsCompartment()
    {
        var service = new NotificationService();
        var comp = new MedicijnCompartiment("Test", "1mg", 1, new List<TimeSpan>());
        bool eventFired = false;
        service.OnNotification += () => eventFired = true;

        service.TriggerManualReminder(0, comp);

        Assert.True(service.IsNotificationActive);
        Assert.Single(service.dueCompartments);
        Assert.Equal(comp, service.dueCompartments[0].Item2);
        Assert.True(eventFired);
    }

    [Fact]
    public void DismissReminder_ResetsNotificationState()
    {
        var service = new NotificationService();
        service.IsNotificationActive = true;
        bool eventFired = false;
        service.OnNotification += () => eventFired = true;

        service.DismissReminder();

        Assert.False(service.IsNotificationActive);
        Assert.NotNull(typeof(NotificationService)
            .GetField("lastDismissedAt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(service));
        Assert.True(eventFired);
    }

    [Fact]
    public void ClearReminders_ResetsAllState()
    {
        var service = new NotificationService();
        service.IsNotificationActive = true;
        service.dueCompartments.Add((1, new MedicijnCompartiment("Test", "1mg", 1, new List<TimeSpan>())));
        bool eventFired = false;
        service.OnNotification += () => eventFired = true;

        service.ClearReminders();

        Assert.False(service.IsNotificationActive);
        Assert.Empty(service.dueCompartments);
        Assert.Null(typeof(NotificationService)
            .GetField("lastDismissedAt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(service));
        Assert.True(eventFired);
    }

    [Fact]
    public void MedicationTaken_InvokesEvent()
    {
        var service = new NotificationService();
        bool eventFired = false;
        service.OnMedicationTaken += () => eventFired = true;

        service.MedicationTaken();

        Assert.True(eventFired);
    }

    [Fact]
    public void Constructor_StartsTimer()
    {
        var service = new NotificationService();
        var timerField = typeof(NotificationService)
            .GetField("_timer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(service) as System.Timers.Timer;
        Assert.NotNull(timerField);
        Assert.True(timerField.Enabled);
    }
}
