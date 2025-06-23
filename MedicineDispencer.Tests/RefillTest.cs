using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class RefillPageTests
{
    [Fact]
    public void SaveRefill_ShouldIncreaseStock_WhenValid()
    {
        // Arrange
        var compartment = new MedicijnCompartiment("Paracetamol", "500mg", 10, new List<TimeSpan> { new TimeSpan(8, 0, 0) });
        CompartmentsData.compartments = new MedicijnCompartiment?[6];
        CompartmentsData.compartments[0] = compartment;

        var page = new TestableRefillPage
        {
            selectedRefillCompartmentIndex = 0,
            refillAmount = 5
        };

        // Act
        page.SaveRefill();

        // Assert
        Assert.Equal(15, CompartmentsData.compartments[0]!.Voorraad);
    }

    private class TestableRefillPage
    {
        public int selectedRefillCompartmentIndex = 0;
        public int refillAmount = 1;
        private MedicijnCompartiment?[] compartments => CompartmentsData.compartments;

        public void SaveRefill()
        {
            if (refillAmount <= 0) return;
            if (compartments[selectedRefillCompartmentIndex] != null)
            {
                compartments[selectedRefillCompartmentIndex]!.VulVoorraadBij(refillAmount);
            }
        }
    }

    public static class CompartmentsData
    {
        public static MedicijnCompartiment?[] compartments = new MedicijnCompartiment?[6];
    }

    public class MedicijnCompartiment
    {
        public string MedicijnNaam { get; private set; }
        public string Dosis { get; private set; }
        public int Voorraad { get; set; }
        public List<TimeSpan> DoseringstijdenPerDag { get; private set; }

        public MedicijnCompartiment(string medicijnNaam, string dosis, int voorraad, List<TimeSpan> doseringstijden)
        {
            MedicijnNaam = medicijnNaam;
            Dosis = dosis;
            Voorraad = voorraad;
            DoseringstijdenPerDag = doseringstijden;
        }

        public void VulVoorraadBij(int aantal) => Voorraad += aantal;
    }

    [Fact]
    public void VulVoorraadBij_IncreasesVoorraad()
    {
        var comp = new MedicijnCompartiment("Test", "1mg", 1, new List<TimeSpan>());
        comp.VulVoorraadBij(5);
        Assert.Equal(6, comp.Voorraad);
    }    
}