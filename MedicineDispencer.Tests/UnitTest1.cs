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
}