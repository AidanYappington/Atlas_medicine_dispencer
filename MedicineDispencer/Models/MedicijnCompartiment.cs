public enum CompartimentStatus { Vergrendeld, Ontgrendeld, Open }

public class MedicijnCompartiment
{
    public string MedicijnNaam { get; set; }
    public string Dosis { get; set; }
    public CompartimentStatus Status { get; set; }
    public int Voorraad { get; set; }
    public List<TimeSpan> DoseringstijdenPerDag { get; set; }
    public DateTime? LaatsteOpeningTijd { get; set; }

    // Parameterless constructor for deserialization
    public MedicijnCompartiment() { }

    public MedicijnCompartiment(string medicijnNaam, string dosis, int voorraad, List<TimeSpan> doseringstijden, CompartimentStatus status = CompartimentStatus.Vergrendeld, DateTime? laatsteOpeningTijd = null)
    {
        MedicijnNaam = medicijnNaam;
        Dosis = dosis;
        Voorraad = voorraad;
        DoseringstijdenPerDag = doseringstijden;
        Status = status;
        LaatsteOpeningTijd = laatsteOpeningTijd;
    }

    public void VoegDoseringstijdToe(int uur, int minuut) =>
        DoseringstijdenPerDag.Add(new TimeSpan(uur, minuut, 0));

    public void VulVoorraadBij(int aantal) => Voorraad += aantal;

    public bool NeemMedicijn()
    {
        if (Voorraad > 0)
        {
            Voorraad--;
            LaatsteOpeningTijd = DateTime.Now;
            return true;
        }
        return false;
    }

    public void ZetStatus(CompartimentStatus nieuweStatus) => Status = nieuweStatus;

    public string VolgendeDoseringstijd()
    {
        if (DoseringstijdenPerDag.Count == 0)
            return "Geen tijden ingesteld";

        DateTime nu = DateTime.Now;
        TimeSpan huidigeTijd = nu.TimeOfDay;

        // Zoek de eerstvolgende tijd die nog moet komen vandaag
        var volgende = DoseringstijdenPerDag
            .Where(t => t > huidigeTijd)
            .OrderBy(t => t)
            .FirstOrDefault();

        DateTime volgendeDosering;

        if (volgende != default)
        {
            volgendeDosering = nu.Date.Add(volgende);
        }
        else
        {
            // Geen tijd meer vandaag, pak de eerste tijd van morgen
            volgende = DoseringstijdenPerDag.OrderBy(t => t).First();
            volgendeDosering = nu.Date.AddDays(1).Add(volgende);
        }

        return volgendeDosering.ToString("dd-MM HH:mm");
    }
}