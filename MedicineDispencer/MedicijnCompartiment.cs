public enum CompartimentStatus { Vergrendeld, Ontgrendeld, Open }

public class MedicijnCompartiment
{
    public string MedicijnNaam { get; private set; }
    public string Dosis { get; private set; }
    public int CompartimentNummer { get; private set; }
    public CompartimentStatus Status { get; private set; }
    public int Voorraad { get; private set; }
    public List<TimeSpan> DoseringstijdenPerDag { get; private set; } = new List<TimeSpan>();
    public DateTime? LaatsteOpeningTijd { get; private set; }

    public MedicijnCompartiment(int nummer, string medicijnNaam, string dosis)
    {
        CompartimentNummer = nummer;
        MedicijnNaam = medicijnNaam;
        Dosis = dosis;
        Status = CompartimentStatus.Vergrendeld;
        Voorraad = 0;
    }

    public void VoegDoseringstijdToe(int uur, int minuut) =>
        DoseringstijdenPerDag.Add(new TimeSpan(uur, minuut, 0));

    public bool IsHetTijdVoorDosering(DateTime nu)
    {
        TimeSpan huidigeTijd = nu.TimeOfDay;

        foreach (TimeSpan tijd in DoseringstijdenPerDag)
        {
            if (Math.Abs((huidigeTijd - tijd).TotalMinutes) <= 5)
                return true;
        }

        return false;
    }

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

        TimeSpan? volgende = null;
        double kortsteWachttijd = double.MaxValue;

        foreach (TimeSpan tijd in DoseringstijdenPerDag)
        {
            double wachttijdMinuten = tijd > huidigeTijd
                ? (tijd - huidigeTijd).TotalMinutes
                : (TimeSpan.FromHours(24) - huidigeTijd + tijd).TotalMinutes;

            if (wachttijdMinuten < kortsteWachttijd)
            {
                kortsteWachttijd = wachttijdMinuten;
                volgende = tijd;
            }
        }

        if (volgende.HasValue)
        {
            DateTime volgendeDosering = volgende.Value > huidigeTijd
                ? nu.Date.Add(volgende.Value)
                : nu.Date.AddDays(1).Add(volgende.Value);

            return volgendeDosering.ToString("dd-MM HH:mm");
        }

        return "Onbekend";
    }
}