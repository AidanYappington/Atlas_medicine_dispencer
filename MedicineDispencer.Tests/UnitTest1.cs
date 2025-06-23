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
}