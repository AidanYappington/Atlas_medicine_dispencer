using System;
using System.Collections.Generic;
using System.Text.Json;
using MedicineDispencer;
using Xunit;

public class UnitTest1
{
    [Fact]
    public void MedicijnCompartiment_Creation_Works()
    {
        var comp = new MedicijnCompartiment("Paracetamol", "500mg", 10, new List<TimeSpan> { new(8, 0, 0) });
        Assert.Equal("Paracetamol", comp.MedicijnNaam);
        Assert.Equal("500mg", comp.Dosis);
        Assert.Equal(10, comp.Voorraad);
        Assert.Single(comp.DoseringstijdenPerDag);
    }

    [Fact]
    public void NeemMedicijn_DecreasesVoorraad()
    {
        var comp = new MedicijnCompartiment("Ibuprofen", "200mg", 5, new List<TimeSpan> { new(9, 0, 0) });
        bool result = comp.NeemMedicijn();
        Assert.True(result);
        Assert.Equal(4, comp.Voorraad);
    }

    [Fact]
    public void NeemMedicijn_ReturnsFalse_WhenEmpty()
    {
        var comp = new MedicijnCompartiment("Ibuprofen", "200mg", 0, new List<TimeSpan> { new(9, 0, 0) });
        bool result = comp.NeemMedicijn();
        Assert.False(result);
        Assert.Equal(0, comp.Voorraad);
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
    public void ZetStatus_ChangesStatus()
    {
        var comp = new MedicijnCompartiment("Test", "1mg", 1, new List<TimeSpan>());
        comp.ZetStatus(CompartimentStatus.Ontgrendeld);
        Assert.Equal(CompartimentStatus.Ontgrendeld, comp.Status);
    }

    [Fact]
    public void VulVoorraadBij_IncreasesVoorraad()
    {
        var comp = new MedicijnCompartiment("Test", "1mg", 1, new List<TimeSpan>());
        comp.VulVoorraadBij(5);
        Assert.Equal(6, comp.Voorraad);
    }

    [Fact]
    public void VolgendeDoseringstijd_ReturnsNextToday()
    {
        var now = DateTime.Now;
        var times = new List<TimeSpan> { now.TimeOfDay.Add(TimeSpan.FromMinutes(10)) };
        var comp = new MedicijnCompartiment("Test", "1mg", 1, times);
        var result = comp.VolgendeDoseringstijd();
        Assert.Contains(now.AddMinutes(10).ToString("dd-MM HH:mm"), result);
    }

    [Fact]
    public void VolgendeDoseringstijd_ReturnsTomorrowIfNoneLeftToday()
    {
        var now = DateTime.Now;
        var times = new List<TimeSpan> { now.TimeOfDay.Subtract(TimeSpan.FromMinutes(10)) };
        var comp = new MedicijnCompartiment("Test", "1mg", 1, times);
        var result = comp.VolgendeDoseringstijd();
        Assert.Contains(now.AddDays(1).Date.Add(times[0]).ToString("dd-MM HH:mm"), result);
    }
}