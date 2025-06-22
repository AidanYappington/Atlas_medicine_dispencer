public enum CompartimentStatus { Vergrendeld, Ontgrendeld, Open }

public class MedicijnCompartiment
{
    public string MedicijnNaam { get; private set; }
    public string Dosis { get; private set; }
    public CompartimentStatus Status { get; private set; }
    public int Voorraad { get; private set; }
    public List<TimeSpan> DoseringstijdenPerDag { get; private set; }
    public DateTime? LaatsteOpeningTijd { get; private set; }

    public MedicijnCompartiment(string medicijnNaam, string dosis, int voorraad, List<TimeSpan> doseringstijden)
    {
        MedicijnNaam = medicijnNaam;
        Dosis = dosis;
        Status = CompartimentStatus.Vergrendeld;
        Voorraad = voorraad;
        DoseringstijdenPerDag = doseringstijden;
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