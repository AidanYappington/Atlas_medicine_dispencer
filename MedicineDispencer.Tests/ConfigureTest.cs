using System;
using System.Collections.Generic;
using Xunit;

public class CompartmentConfigurationTests
{
    [Fact]
    public void OnSwitchCompartment_ShouldLoadExistingValues()
    {
        // Arrange
        CompartmentsData.compartments = new MedicijnCompartiment?[4];
        CompartmentsData.compartments[1] = new MedicijnCompartiment(
            "Aspirine",
            "500mg",
            20,
            new List<TimeSpan> { new TimeSpan(9, 0, 0), new TimeSpan(21, 0, 0) }
        );

        var component = new TestableCompartmentComponent { SelectedIndex = 1 };

        // Act
        component.InvokeOnSwitchCompartment();

        // Assert
        Assert.Equal("Aspirine", component.ConfigMedicationName);
        Assert.Equal("500mg", component.ConfigMedicationDosage);
        Assert.Equal(20, component.ConfigMedicationStock);
        Assert.Equal(2, component.ConfigDosingTimes.Count);
    }

    [Fact]
    public void UpdateDosingTime_ShouldUpdateSpecificTime()
    {
        // Arrange
        var component = new TestableCompartmentComponent
        {
            ConfigDosingTimes = new List<TimeSpan> { new TimeSpan(8, 0, 0) }
        };

        // Act
        component.UpdateDosingTime(0, "10:30");

        // Assert
        Assert.Equal(new TimeSpan(10, 30, 0), component.ConfigDosingTimes[0]);
    }

    [Fact]
    public void AddDosingTime_ShouldAppendDefaultTime()
    {
        var component = new TestableCompartmentComponent();

        component.AddDosingTime();

        Assert.Single(component.ConfigDosingTimes);
        Assert.Contains(new TimeSpan(8, 0, 0), component.ConfigDosingTimes);
    }

    [Fact]
    public void VoegDoseringstijdToe_AddsTime()
    {
        var comp = new MedicijnCompartiment("Test", "1mg", 1, new List<TimeSpan>());
        comp.VoegDoseringstijdToe(10, 30);
        Assert.Single(comp.DoseringstijdenPerDag);
        Assert.Equal(new TimeSpan(10, 30, 0), comp.DoseringstijdenPerDag[0]);
    }    

    [Fact]
    public void RemoveDosingTime_ShouldRemoveTimeAtIndex()
    {
        var component = new TestableCompartmentComponent
        {
            ConfigDosingTimes = new List<TimeSpan> { new TimeSpan(9, 0, 0), new TimeSpan(18, 0, 0) }
        };

        component.RemoveDosingTime(0);

        Assert.Single(component.ConfigDosingTimes);
        Assert.Equal(new TimeSpan(18, 0, 0), component.ConfigDosingTimes[0]);
    }

    private class TestableCompartmentComponent
    {
        public int SelectedIndex { get; set; } = 0;
        public string ConfigMedicationName = string.Empty;
        public string ConfigMedicationDosage = string.Empty;
        public List<TimeSpan> ConfigDosingTimes = new();
        public int ConfigMedicationStock = 0;

        public void InvokeOnSwitchCompartment()
        {
            var c = CompartmentsData.compartments[SelectedIndex];
            ConfigDosingTimes = c?.DoseringstijdenPerDag?.ToList() ?? new List<TimeSpan> { new TimeSpan(8, 0, 0) };
            ConfigMedicationName = c?.MedicijnNaam ?? "";
            ConfigMedicationDosage = c?.Dosis ?? "";
            ConfigMedicationStock = c?.Voorraad ?? 0;
        }

        public void AddDosingTime()
        {
            ConfigDosingTimes.Add(new TimeSpan(8, 0, 0));
        }

        public void RemoveDosingTime(int index)
        {
            if (index >= 0 && index < ConfigDosingTimes.Count)
                ConfigDosingTimes.RemoveAt(index);
        }

        public void UpdateDosingTime(int index, string? timeString)
        {
            if (timeString != null && index >= 0 && index < ConfigDosingTimes.Count)
            {
                string[] parts = timeString.Split(':');
                if (parts.Length >= 2 && int.TryParse(parts[0], out int hours) && int.TryParse(parts[1], out int minutes))
                {
                    ConfigDosingTimes[index] = new TimeSpan(hours, minutes, 0);
                }
            }
        }
    }
}