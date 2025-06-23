using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using MedicineDispencer.Services;
using System.Timers;

public class NotificationServiceTests
{
    [Fact]
    public void TriggerManualReminder_ShouldSetNotificationAndInvokeEvent()
    {
        // Arrange
        var service = new NotificationServiceStub();
        var compartment = new MedicijnCompartiment("Test", "1mg", 1, new List<TimeSpan>());
        bool eventTriggered = false;
        service.OnNotification += () => eventTriggered = true;

        // Act
        service.TriggerManualReminder(0, compartment);

        // Assert
        Assert.True(service.IsNotificationActive);
        Assert.Single(service.dueCompartments);
        Assert.Equal((1, compartment), service.dueCompartments.First());
        Assert.True(eventTriggered);
    }

    [Fact]
    public void DismissReminder_ShouldClearNotificationState()
    {
        // Arrange
        var service = new NotificationServiceStub();
        service.IsNotificationActive = true;
        bool eventTriggered = false;
        service.OnNotification += () => eventTriggered = true;

        // Act
        service.DismissReminder();

        // Assert
        Assert.False(service.IsNotificationActive);
        Assert.True(eventTriggered);
    }

    [Fact]
    public void ClearReminders_ShouldClearAllDueCompartments()
    {
        // Arrange
        var service = new NotificationServiceStub();
        service.dueCompartments.Add((1, new MedicijnCompartiment("Test", "1mg", 1, new List<TimeSpan>())));
        service.IsNotificationActive = true;

        // Act
        service.ClearReminders();

        // Assert
        Assert.Empty(service.dueCompartments);
        Assert.False(service.IsNotificationActive);
    }

    [Fact]
    public void MedicationTaken_ShouldInvokeEvent()
    {
        // Arrange
        var service = new NotificationServiceStub();
        bool eventInvoked = false;
        service.OnMedicationTaken += () => eventInvoked = true;

        // Act
        service.MedicationTaken();

        // Assert
        Assert.True(eventInvoked);
    }

    // Subclass disables timer and ServoService
    private class NotificationServiceStub : NotificationService
    {
        public NotificationServiceStub()
        {
            typeof(NotificationService).GetField("_timer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, new System.Timers.Timer { Enabled = false });
        }
    }
}
