using System;
using System.Collections.Generic;
using System.Threading;

namespace ThuisMedicijnDispenser
{
    // Enumeratie voor de status van het dispenser-compartiment
    enum CompartimentStatus
    {
        Vergrendeld,
        Ontgrendeld,
        Open,
        Gesloten
    }
    
    // Enumeratie voor licht-indicaties
    enum LichtStatus
    {
        Uit,
        Rood,
        Groen,
        Geel,
        Knipperend
    }

    // Klasse voor een fysiek compartiment in de dispenser
    class MedicijnCompartiment
    {
        public string MedicijnNaam { get; private set; }
        public string Dosis { get; private set; }
        public int CompartimentNummer { get; private set; }
        public CompartimentStatus Status { get; private set; }
        public LichtStatus Licht { get; private set; }
        public int Voorraad { get; private set; }
        public List<TimeSpan> DoseringstijdenPerDag { get; private set; }
        public DateTime? LaatsteOpeningTijd { get; private set; }

        public MedicijnCompartiment(int nummer, string medicijnNaam, string dosis)
        {
            CompartimentNummer = nummer;
            MedicijnNaam = medicijnNaam;
            Dosis = dosis;
            Status = CompartimentStatus.Vergrendeld;
            Licht = LichtStatus.Uit;
            Voorraad = 0;
            DoseringstijdenPerDag = new List<TimeSpan>();
            LaatsteOpeningTijd = null;
        }

        public void VoegDoseringstijdToe(int uur, int minuut)
        {
            DoseringstijdenPerDag.Add(new TimeSpan(uur, minuut, 0));
        }

        public bool IsHetTijdVoorDosering(DateTime nu)
        {
            TimeSpan huidigeTijd = nu.TimeOfDay;
            
            foreach (TimeSpan doseringstijd in DoseringstijdenPerDag)
            {
                // Controleer of het binnen 30 minuten van een dosering is
                double verschilMinuten = Math.Abs((huidigeTijd - doseringstijd).TotalMinutes);
                
                if (verschilMinuten <= 30)
                {
                    return true;
                }
            }
            
            return false;
        }

        public void VulVoorraadBij(int aantal)
        {
            Voorraad += aantal;
        }

        public bool NeemMedicijn()
        {
            if (Voorraad > 0 && Status == CompartimentStatus.Open)
            {
                Voorraad--;
                LaatsteOpeningTijd = DateTime.Now;
                return true;
            }
            return false;
        }

        public void ZetStatus(CompartimentStatus nieuweStatus)
        {
            Status = nieuweStatus;
            
            // Update licht gebaseerd op nieuwe status
            switch (Status)
            {
                case CompartimentStatus.Vergrendeld:
                    Licht = LichtStatus.Rood;
                    break;
                case CompartimentStatus.Ontgrendeld:
                    Licht = LichtStatus.Knipperend;
                    break;
                case CompartimentStatus.Open:
                    Licht = LichtStatus.Groen;
                    break;
                case CompartimentStatus.Gesloten:
                    Licht = LichtStatus.Uit;
                    break;
            }
        }

        public string VolgendeDoseringstijd()
        {
            if (DoseringstijdenPerDag.Count == 0)
                return "Geen doseringstijden ingesteld";

            DateTime nu = DateTime.Now;
            TimeSpan huidigeTijd = nu.TimeOfDay;
            
            // Zoek het eerstvolgende doseringsmoment
            TimeSpan? volgende = null;
            double kortsteWachttijd = double.MaxValue;
            
            foreach (TimeSpan doseringstijd in DoseringstijdenPerDag)
            {
                // Bereken wachttijd (vandaag of morgen)
                double wachttijdMinuten;
                
                if (doseringstijd > huidigeTijd)
                {
                    // Later vandaag
                    wachttijdMinuten = (doseringstijd - huidigeTijd).TotalMinutes;
                }
                else
                {
                    // Morgen
                    wachttijdMinuten = (TimeSpan.FromHours(24) - huidigeTijd + doseringstijd).TotalMinutes;
                }
                
                if (wachttijdMinuten < kortsteWachttijd)
                {
                    kortsteWachttijd = wachttijdMinuten;
                    volgende = doseringstijd;
                }
            }
            
            if (volgende.HasValue)
            {
                // Bereken datum en tijd van de volgende dosis
                DateTime volgendeDosering;
                
                if (volgende.Value > huidigeTijd)
                {
                    // Later vandaag
                    volgendeDosering = nu.Date.Add(volgende.Value);
                }
                else
                {
                    // Morgen
                    volgendeDosering = nu.Date.AddDays(1).Add(volgende.Value);
                }
                
                return volgendeDosering.ToString("dd-MM HH:mm");
            }
            
            return "Onbekend";
        }
    }

    // Hoofdklasse voor de medicijndispenser
    class FysiekeMedicijnDispenser
    {
        private List<MedicijnCompartiment> compartimenten;
        private System.Timers.Timer controleTimer;
        private List<(int compartimentNr, DateTime tijdstip, bool genomen)> innameLogs;
        private bool alarmActief = false;
        private int actieveCompartimentIndex = -1;

        public FysiekeMedicijnDispenser(int aantalCompartimenten = 4)
        {
            // Initialiseer de dispenser met het opgegeven aantal medicijncompartimenten
            compartimenten = new List<MedicijnCompartiment>();
            for (int i = 0; i < aantalCompartimenten; i++)
            {
                compartimenten.Add(null); // Lege sloten
            }
            
            innameLogs = new List<(int, DateTime, bool)>();
            
            // Timer instellen om elke minuut te controleren
            controleTimer = new System.Timers.Timer(60 * 1000); // 1 minuut
            controleTimer.Elapsed += (sender, e) => ControleerDoseringstijden();
            controleTimer.AutoReset = true;
            controleTimer.Enabled = true;
        }

        // Methode om de fysieke dispenser te configureren
        public void ConfigureerCompartiment(int compartimentNr, string medicijnNaam, string dosis)
        {
            if (compartimentNr >= 0 && compartimentNr < compartimenten.Count)
            {
                compartimenten[compartimentNr] = new MedicijnCompartiment(compartimentNr + 1, medicijnNaam, dosis);
                Console.WriteLine($"Compartiment {compartimentNr + 1} geconfigureerd voor {medicijnNaam} ({dosis})");
            }
            else
            {
                Console.WriteLine($"Fout: Compartiment {compartimentNr + 1} bestaat niet.");
            }
        }

        // Voeg een doseringstijd toe aan een compartiment
        public void VoegDoseringstijdToe(int compartimentNr, int uur, int minuut)
        {
            if (compartimentNr >= 0 && compartimentNr < compartimenten.Count && compartimenten[compartimentNr] != null)
            {
                compartimenten[compartimentNr].VoegDoseringstijdToe(uur, minuut);
                Console.WriteLine($"Doseringstijd {uur:D2}:{minuut:D2} toegevoegd voor compartiment {compartimentNr + 1}");
            }
            else
            {
                Console.WriteLine($"Fout: Compartiment {compartimentNr + 1} bestaat niet of is niet geconfigureerd.");
            }
        }

        // Vul een compartiment bij
        public void VulCompartimentBij(int compartimentNr, int aantal)
        {
            if (compartimentNr >= 0 && compartimentNr < compartimenten.Count && compartimenten[compartimentNr] != null)
            {
                compartimenten[compartimentNr].VulVoorraadBij(aantal);
                Console.WriteLine($"Compartiment {compartimentNr + 1} bijgevuld met {aantal} {compartimenten[compartimentNr].MedicijnNaam}");
            }
            else
            {
                Console.WriteLine($"Fout: Compartiment {compartimentNr + 1} bestaat niet of is niet geconfigureerd.");
            }
        }

        // Timer-functie om te controleren of er medicijnen moeten worden ingenomen
        private void ControleerDoseringstijden()
        {
            DateTime nu = DateTime.Now;
            bool medicijnNodig = false;
            
            // Als er al een alarm actief is, niets doen
            if (alarmActief)
                return;
            
            for (int i = 0; i < compartimenten.Count; i++)
            {
                if (compartimenten[i] != null && compartimenten[i].IsHetTijdVoorDosering(nu) && compartimenten[i].Status == CompartimentStatus.Vergrendeld)
                {
                    medicijnNodig = true;
                    actieveCompartimentIndex = i;
                    
                    // Toon alarm op scherm
                    alarmActief = true;
                    Console.Clear();
                    
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n╔════════════════════════════════════════════╗");
                    Console.WriteLine("║            MEDICIJN HERINNERING            ║");
                    Console.WriteLine("╚════════════════════════════════════════════╝");
                    Console.ResetColor();
                    
                    Console.WriteLine($"\nHet is tijd om uw medicijn te nemen:");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"► {compartimenten[i].MedicijnNaam} ({compartimenten[i].Dosis})");
                    Console.WriteLine($"► Compartiment {compartimenten[i].CompartimentNummer}");
                    Console.WriteLine("\n[ROOD LICHT] Druk op de knop om te ontgrendelen");
                    Console.ResetColor();
                    
                    // Speel alarm geluid af
                    SpeelAlarm();
                    
                    // In echte hardware zou de gebruiker op een fysieke knop drukken
                    // Hier simuleren we dat met een toetsenbordinvoer
                    Console.WriteLine("\nDruk op 'O' om het compartiment te ontgrendelen.");
                    Console.WriteLine("Druk op 'S' om het alarm te stoppen en later te herinneren.");
                    
                    // Start een aparte thread om op input te wachten, zodat de rest van het programma kan doorgaan
                    Thread inputThread = new Thread(() => WachtOpOntgrendelInput(i));
                    inputThread.IsBackground = true;
                    inputThread.Start();
                    
                    // Stop na het eerste medicijn dat moet worden ingenomen
                    // In een echte dispenser zou er één compartiment tegelijk opengaan
                    break;
                }
            }
        }

        // Methode om gebruikersinput te verwerken voor het alarm
        private void WachtOpOntgrendelInput(int compartimentIndex)
        {
            bool wachtOpInput = true;
            while (wachtOpInput && alarmActief)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.O)
                    {
                        // Ontgrendel het compartiment
                        OntvergrendelCompartiment(compartimentIndex);
                        wachtOpInput = false;
                    }
                    else if (key.Key == ConsoleKey.S)
                    {
                        // Stop het alarm en stel uit
                        alarmActief = false;
                        actieveCompartimentIndex = -1;
                        Console.Clear();
                        ToonStatus();
                        wachtOpInput = false;
                    }
                }
                
                // Korte pauze om CPU-gebruik te beperken
                Thread.Sleep(100);
            }
        }

        // Methode om een compartiment te ontgrendelen
        private void OntvergrendelCompartiment(int compartimentIndex)
        {
            if (compartimentIndex >= 0 && compartimentIndex < compartimenten.Count && compartimenten[compartimentIndex] != null)
            {
                alarmActief = false;
                Console.Clear();
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nCompartiment {compartimenten[compartimentIndex].CompartimentNummer} wordt ontgrendeld...");
                Console.ResetColor();
                
                // Simuleer het ontgrendelen
                Thread.Sleep(1000);
                
                // Zet status op ontgrendeld, dit wijzigt ook het licht naar knipperend
                compartimenten[compartimentIndex].ZetStatus(CompartimentStatus.Ontgrendeld);
                
                // Beep om ontgrendeling te bevestigen
                try
                {
                    Console.Beep(1800, 300);
                }
                catch { }
                
                // Toon instructies voor het openen
                Console.WriteLine("\nHet compartiment is ONTGRENDELD [KNIPPEREND LICHT]");
                Console.WriteLine("Druk op 'A' om het compartiment te openen.");
                
                // Wacht tot de gebruiker het compartiment opent
                bool wachtOpOpenen = true;
                while (wachtOpOpenen)
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.A)
                        {
                            OpenCompartiment(compartimentIndex);
                            wachtOpOpenen = false;
                        }
                    }
                    
                    Thread.Sleep(100);
                }
            }
        }

        // Methode om een ontgrendeld compartiment te openen
        private void OpenCompartiment(int compartimentIndex)
        {
            if (compartimentIndex >= 0 && 
                compartimentIndex < compartimenten.Count && 
                compartimenten[compartimentIndex] != null &&
                compartimenten[compartimentIndex].Status == CompartimentStatus.Ontgrendeld)
            {
                Console.Clear();
                
                Console.WriteLine($"\nCompartiment {compartimenten[compartimentIndex].CompartimentNummer} wordt geopend...");
                
                // Simuleer het openen
                Thread.Sleep(1000);
                
                // Zet status op open, dit wijzigt ook het licht naar groen
                compartimenten[compartimentIndex].ZetStatus(CompartimentStatus.Open);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nCompartiment is nu OPEN [GROEN LICHT]");
                Console.WriteLine($"Neem uw {compartimenten[compartimentIndex].MedicijnNaam} ({compartimenten[compartimentIndex].Dosis})");
                Console.ResetColor();
                
                // Beep om opening te bevestigen
                try
                {
                    Console.Beep(2000, 150);
                    Thread.Sleep(50);
                    Console.Beep(2400, 300);
                }
                catch { }
                
                // Wacht tot de gebruiker het medicijn heeft genomen
                Console.WriteLine("\nDruk op 'N' nadat u het medicijn heeft genomen.");
                Console.WriteLine("Druk op 'S' om het compartiment te sluiten zonder medicijn te nemen.");
                
                bool wachtOpNemen = true;
                while (wachtOpNemen)
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.N)
                        {
                            NeemMedicijn(compartimentIndex);
                            wachtOpNemen = false;
                        }
                        else if (key.Key == ConsoleKey.S)
                        {
                            SluitCompartiment(compartimentIndex, false);
                            wachtOpNemen = false;
                        }
                    }
                    
                    Thread.Sleep(100);
                }
            }
        }

        // Methode om een medicijn te nemen
        private void NeemMedicijn(int compartimentIndex)
        {
            if (compartimentIndex >= 0 && 
                compartimentIndex < compartimenten.Count && 
                compartimenten[compartimentIndex] != null &&
                compartimenten[compartimentIndex].Status == CompartimentStatus.Open)
            {
                // Neem het medicijn (verminder voorraad)
                bool succeed = compartimenten[compartimentIndex].NeemMedicijn();
                
                if (succeed)
                {
                    // Log de succesvolle inname
                    innameLogs.Add((compartimentIndex, DateTime.Now, true));
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\nU heeft {compartimenten[compartimentIndex].MedicijnNaam} succesvol ingenomen!");
                    Console.ResetColor();
                    
                    Console.WriteLine($"Resterende voorraad: {compartimenten[compartimentIndex].Voorraad}");
                    
                    // Beep om succesvolle inname te bevestigen
                    try
                    {
                        // Positieve melodie
                        Console.Beep(1800, 150);
                        Thread.Sleep(50);
                        Console.Beep(2000, 150);
                        Thread.Sleep(50);
                        Console.Beep(2400, 300);
                    }
                    catch { }
                }
                else
                {
                    // Log de mislukte inname
                    innameLogs.Add((compartimentIndex, DateTime.Now, false));
                    
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\nFout: Geen {compartimenten[compartimentIndex].MedicijnNaam} meer in dit compartiment!");
                    Console.WriteLine("Neem contact op met uw verzorger om het compartiment bij te vullen.");
                    Console.ResetColor();
                    
                    // Beep om mislukte inname te melden
                    try
                    {
                        Console.Beep(800, 300);
                        Thread.Sleep(100);
                        Console.Beep(600, 500);
                    }
                    catch { }
                }
                
                // Sluit het compartiment altijd na de poging
                Thread.Sleep(1500);
                SluitCompartiment(compartimentIndex, succeed);
            }
        }

        // Methode om een compartiment te sluiten
        private void SluitCompartiment(int compartimentIndex, bool medicijnGenomen)
        {
            if (compartimentIndex >= 0 && 
                compartimentIndex < compartimenten.Count && 
                compartimenten[compartimentIndex] != null &&
                (compartimenten[compartimentIndex].Status == CompartimentStatus.Open || 
                 compartimenten[compartimentIndex].Status == CompartimentStatus.Ontgrendeld))
            {
                Console.WriteLine("\nCompartiment wordt gesloten...");
                
                // Simuleer het sluiten
                Thread.Sleep(1000);
                
                // Zet status op vergrendeld
                compartimenten[compartimentIndex].ZetStatus(CompartimentStatus.Vergrendeld);
                
                Console.WriteLine("Compartiment is nu VERGRENDELD [ROOD LICHT]");
                
                // Als het medicijn niet was genomen, log dit
                if (!medicijnGenomen)
                {
                    innameLogs.Add((compartimentIndex, DateTime.Now, false));
                }
                
                Thread.Sleep(1000);
                actieveCompartimentIndex = -1;
                Console.Clear();
                ToonStatus();
            }
        }

        // Methode om het alarm af te spelen
        private void SpeelAlarm()
        {
            try
            {
                // SOS patroon (internationaal noodsignaal)
                // Drie korte tonen
                for (int i = 0; i < 3; i++)
                {
                    Console.Beep(2000, 200);
                    Thread.Sleep(100);
                }
                
                Thread.Sleep(300);
                
                // Drie lange tonen
                for (int i = 0; i < 3; i++)
                {
                    Console.Beep(2000, 500);
                    Thread.Sleep(100);
                }
                
                Thread.Sleep(300);
                
                // Drie korte tonen
                for (int i = 0; i < 3; i++)
                {
                    Console.Beep(2000, 200);
                    Thread.Sleep(100);
                }
            }
            catch { }
        }

        // Toon de status van alle compartimenten
        public void ToonStatus()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════╗");
            Console.WriteLine("║        MEDICIJN DISPENSER STATUS           ║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            
            bool legeDispenser = true;
            
            for (int i = 0; i < compartimenten.Count; i++)
            {
                Console.WriteLine($"\nCompartiment {i + 1}:");
                
                if (compartimenten[i] != null)
                {
                    legeDispenser = false;
                    
                    // Basis informatie
                    Console.WriteLine($"Medicijn: {compartimenten[i].MedicijnNaam} ({compartimenten[i].Dosis})");
                    Console.WriteLine($"Voorraad: {compartimenten[i].Voorraad} tabletten");
                    
                    // Status met kleurcode
                    Console.Write("Status: ");
                    
                    switch (compartimenten[i].Status)
                    {
                        case CompartimentStatus.Vergrendeld:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("VERGRENDELD [ROOD]");
                            Console.ResetColor();
                            break;
                        case CompartimentStatus.Ontgrendeld:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("ONTGRENDELD [KNIPPEREND]");
                            Console.ResetColor();
                            break;
                        case CompartimentStatus.Open:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("OPEN [GROEN]");
                            Console.ResetColor();
                            break;
                        default:
                            Console.WriteLine(compartimenten[i].Status.ToString());
                            break;
                    }
                    
                    // Volgende dosering
                    Console.WriteLine($"Volgende dosering: {compartimenten[i].VolgendeDoseringstijd()}");
                    Console.WriteLine(new string('-', 50));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("Niet geconfigureerd");
                    Console.ResetColor();
                    Console.WriteLine(new string('-', 50));
                }
            }
            
            if (legeDispenser)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nGeen medicijnen geconfigureerd in deze dispenser.");
                Console.WriteLine("Gebruik optie 3 om medicijncompartimenten te configureren.");
                Console.ResetColor();
            }
        }

        // Toon het logboek van innames
        public void ToonInnameLogs()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════╗");
            Console.WriteLine("║           MEDICIJN INNAME LOG              ║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            
            if (innameLogs.Count == 0)
            {
                Console.WriteLine("\nGeen medicijnen ingenomen.");
                return;
            }
            
            Console.WriteLine("\n" + new string('-', 60));
            Console.WriteLine($"{"Tijd",-20} {"Compartiment",-15} {"Medicijn",-15} {"Status",-10}");
            Console.WriteLine(new string('-', 60));
            
            foreach (var log in innameLogs)
            {
                string medicijnNaam = compartimenten[log.compartimentNr] != null 
                    ? compartimenten[log.compartimentNr].MedicijnNaam 
                    : "Onbekend";
                    
                string status = log.genomen ? "Genomen" : "Gemist";
                
                Console.WriteLine($"{log.tijdstip.ToString("dd-MM-yyyy HH:mm"),-20} " +
                                  $"{(log.compartimentNr + 1).ToString(),-15} " +
                                  $"{medicijnNaam,-15} " +
                                  $"{status,-10}");
            }
            
            Console.WriteLine(new string('-', 60));
        }

        // Aantal compartimenten
        public int AantalCompartimenten
        {
            get { return compartimenten.Count; }
        }

        // Deze methode zou in een echte dispenser via een extern systeem worden aangeroepen
        public void NoodontgrendelingAlleCompartimenten()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n╔════════════════════════════════════════════╗");
            Console.WriteLine("║             NOODONTGRENDELING              ║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.ResetColor();
            
            Console.WriteLine("\nLet op: Alle compartimenten worden ontgrendeld!");
            Console.WriteLine("Dit is alleen bedoeld voor noodgevallen.");
            Console.WriteLine("Weet u zeker dat u wilt doorgaan? (j/n)");
            
            string bevestiging = Console.ReadLine().ToLower();
            if (bevestiging == "j" || bevestiging == "ja")
            {
                Console.WriteLine("\nAllecompartimenten worden ontgrendeld...");
                
                // Simuleer ontgrendeling
                try
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Console.Beep(2000, 200);
                        Thread.Sleep(100);
                    }
                }
                catch { }
                
                Thread.Sleep(1000);
                
                // Ontgrendel alle compartimenten
                for (int i = 0; i < compartimenten.Count; i++)
                {
                    if (compartimenten[i] != null)
                    {
                        compartimenten[i].ZetStatus(CompartimentStatus.Ontgrendeld);
                        Console.WriteLine($"Compartiment {i + 1} ontgrendeld.");
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nAlle compartimenten zijn nu ontgrendeld.");
                Console.WriteLine("Sluit de compartimenten na gebruik!");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("Noodontgrendeling geannuleerd.");
            }
        }
        
        // Stop alle timers voor het veilig afsluiten van het programma
        public void StopDispenser()
        {
            controleTimer.Stop();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Thuis Medicijn Dispenser";
            
            Console.WriteLine("╔══════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                      ║");
            Console.WriteLine("║            THUIS MEDICIJN DISPENSER                  ║");
            Console.WriteLine("║                                                      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════╝");
            
            // Maak een medicijndispenser met 4 compartimenten
            FysiekeMedicijnDispenser dispenser = new FysiekeMedicijnDispenser(4);
            
            // Test beep bij opstart
            try
            {
                Console.WriteLine("\nSysteemtest: controleer of u een geluidssignaal hoort...");
                Console.Beep(1000, 500);
                Thread.Sleep(500);
                Console.WriteLine("Geluidstest voltooid.");
            }
            catch (Exception)
            {
                Console.WriteLine("Let op: Geluidssignalen worden niet ondersteund op dit systeem.");
            }
            
            // Vraag om de dispenser in te stellen als deze nog leeg is
            Console.WriteLine("\nWilt u voorbeeldmedicijnen instellen? (j/n)");
            if (Console.ReadLine().ToLower() == "j")
            {
                SetupVoorbeeldDispenser(dispenser);
            }
            
            bool doorgaan = true;
            
            while (doorgaan)
            {
                Console.WriteLine("\n╔════════════════════════════════════════════╗");
                Console.WriteLine("║        THUIS MEDICIJN DISPENSER MENU       ║");
                Console.WriteLine("╚════════════════════════════════════════════╝");
                
                Console.WriteLine("\nKies een optie:");
                Console.WriteLine("1. Toon status van alle compartimenten");
                Console.WriteLine("2. Medicijninname logboek bekijken");
                Console.WriteLine("3. Configureer een compartiment (beheerder)");
                Console.WriteLine("4. Vul een compartiment bij (beheerder)");
                Console.WriteLine("5. Noodontgrendeling (alleen voor noodgevallen)");
                Console.WriteLine("0. Dispenser uitschakelen");
                
                Console.Write("\nUw keuze: ");
                string keuze = Console.ReadLine();
                Console.Clear();
                
                switch (keuze)
                {
                    case "1":
                        dispenser.ToonStatus();
                        break;
                    
                    case "2":
                        dispenser.ToonInnameLogs();
                        break;
                    
                    case "3":
                        ConfigureerCompartiment(dispenser);
                        break;
                    
                    case "4":
                        VulCompartimentBij(dispenser);
                        break;
                    
                    case "5":
                        dispenser.NoodontgrendelingAlleCompartimenten();
                        break;
                    
                    case "0":
                        doorgaan = false;
                        dispenser.StopDispenser();
                        Console.WriteLine("Medicijndispenser wordt uitgeschakeld...");
                        Thread.Sleep(1000);
                        Console.WriteLine("Programma beëindigd. Tot ziens!");
                        break;
                    
                    default:
                        Console.WriteLine("Ongeldige keuze. Probeer opnieuw.");
                        break;
                }
                
                if (doorgaan)
                {
                    Console.WriteLine("\nDruk op Enter om terug te gaan naar het hoofdmenu...");
                    Console.ReadLine();
                    Console.Clear();
                }
            }
        }
        
        // Helper methode om een dispenser in te stellen
        static void SetupVoorbeeldDispenser(FysiekeMedicijnDispenser dispenser)
        {
            // Compartiment 1: Paracetamol (3x daags)
            dispenser.ConfigureerCompartiment(0, "Paracetamol", "500mg");
            dispenser.VoegDoseringstijdToe(0, 12, 49);  // 's Ochtends om 8:00
            dispenser.VoegDoseringstijdToe(0, 13, 0); // 's Middags om 13:00
            dispenser.VoegDoseringstijdToe(0, 20, 0); // 's Avonds om 20:00
            dispenser.VulCompartimentBij(0, 30);
            
            // Compartiment 2: Vitamine D (1x daags)
            dispenser.ConfigureerCompartiment(1, "Vitamine D", "25mcg");
            dispenser.VoegDoseringstijdToe(1, 8, 0);  // 's Ochtends om 8:00
            dispenser.VulCompartimentBij(1, 60);
            
            // Compartiment 3: Bloeddrukmedicatie (2x daags)
            dispenser.ConfigureerCompartiment(2, "Lisinopril", "10mg");
            dispenser.VoegDoseringstijdToe(2, 8, 0);  // 's Ochtends om 8:00
            dispenser.VoegDoseringstijdToe(2, 20, 0); // 's Avonds om 20:00
            dispenser.VulCompartimentBij(2, 28);
            
            // Compartiment 4: Demo medicijn (voor testen, stelt in op huidige tijd)
            dispenser.ConfigureerCompartiment(3, "DemoPil", "50mg");
            
            // Voeg een innamemoment toe dicht bij de huidige tijd voor demonstratie
            DateTime nu = DateTime.Now;
            // 1 minuut later voor demo
            DateTime demoTijd = nu.AddMinutes(1);
            dispenser.VoegDoseringstijdToe(3, demoTijd.Hour, demoTijd.Minute);
            dispenser.VulCompartimentBij(3, 10);
            
            Console.WriteLine("\nAlle voorbeeldmedicijnen zijn ingesteld!");
            Console.WriteLine($"Demo medicijn is ingesteld voor {demoTijd.ToString("HH:mm")}.");
        }
        
        // Helper methode om een compartiment te configureren
        static void ConfigureerCompartiment(FysiekeMedicijnDispenser dispenser)
        {
            Console.WriteLine("\n╔════════════════════════════════════════════╗");
            Console.WriteLine("║      COMPARTIMENT CONFIGURATIE MENU        ║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            
            // Toon beschikbare compartimenten
            Console.WriteLine("\nBeschikbare compartimenten (1-" + dispenser.AantalCompartimenten + "):");
            
            // Vraag welk compartiment
            Console.Write("\nWelk compartiment wilt u configureren? ");
            if (int.TryParse(Console.ReadLine(), out int compartimentNr) && 
                compartimentNr >= 1 && compartimentNr <= dispenser.AantalCompartimenten)
            {
                // C# indexeert vanaf 0, maar gebruikers zien compartimenten vanaf 1
                int index = compartimentNr - 1;
                
                Console.Write("Voer de naam van het medicijn in: ");
                string medicijnNaam = Console.ReadLine();
                
                Console.Write("Voer de dosis in (bijv. 500mg): ");
                string dosis = Console.ReadLine();
                
                // Configureer het compartiment
                dispenser.ConfigureerCompartiment(index, medicijnNaam, dosis);
                
                // Vraag om doseringstijden
                Console.WriteLine("\nVoeg doseringstijden toe (24-uurs notatie).");
                Console.WriteLine("Laat leeg en druk op Enter om te stoppen met toevoegen.");
                
                bool doorgaan = true;
                while (doorgaan)
                {
                    Console.Write("Tijdstip (UU:MM): ");
                    string tijdstip = Console.ReadLine();
                    
                    if (string.IsNullOrWhiteSpace(tijdstip))
                    {
                        doorgaan = false;
                    }
                    else
                    {
                        string[] delen = tijdstip.Split(':');
                        if (delen.Length == 2 && 
                            int.TryParse(delen[0], out int uur) && 
                            int.TryParse(delen[1], out int minuut))
                        {
                            if (uur >= 0 && uur < 24 && minuut >= 0 && minuut < 60)
                            {
                                dispenser.VoegDoseringstijdToe(index, uur, minuut);
                            }
                            else
                            {
                                Console.WriteLine("Ongeldig tijdstip. Uren moeten tussen 0-23 liggen en minuten tussen 0-59.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Ongeldig formaat. Gebruik UU:MM (bijv. 08:30).");
                        }
                    }
                }
                
                // Vraag om beginvoorraad
                Console.Write($"\nBeginvoorraad voor {medicijnNaam}: ");
                if (int.TryParse(Console.ReadLine(), out int voorraad) && voorraad > 0)
                {
                    dispenser.VulCompartimentBij(index, voorraad);
                }
                else
                {
                    Console.WriteLine("Ongeldige voorraad. Voorraad op 0 gezet.");
                }
            }
            else
            {
                Console.WriteLine($"Ongeldig compartimentnummer. Kies een nummer tussen 1 en {dispenser.AantalCompartimenten}.");
            }
        }
        
        // Helper methode om een compartiment bij te vullen
        static void VulCompartimentBij(FysiekeMedicijnDispenser dispenser)
        {
            Console.WriteLine("\n╔════════════════════════════════════════════╗");
            Console.WriteLine("║      COMPARTIMENT BIJVULLEN MENU           ║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            
            // Toon beschikbare compartimenten
            dispenser.ToonStatus();
            
            // Vraag welk compartiment
            Console.Write("\nWelk compartiment wilt u bijvullen? ");
            if (int.TryParse(Console.ReadLine(), out int compartimentNr) && 
                compartimentNr >= 1 && compartimentNr <= dispenser.AantalCompartimenten)
            {
                // C# indexeert vanaf 0, maar gebruikers zien compartimenten vanaf 1
                int index = compartimentNr - 1;
                
                Console.Write("Hoeveel medicijnen wilt u toevoegen? ");
                if (int.TryParse(Console.ReadLine(), out int aantal) && aantal > 0)
                {
                    dispenser.VulCompartimentBij(index, aantal);
                }
                else
                {
                    Console.WriteLine("Ongeldige hoeveelheid.");
                }
            }
            else
            {
                Console.WriteLine($"Ongeldig compartimentnummer. Kies een nummer tussen 1 en {dispenser.AantalCompartimenten}.");
            }
        }
    }
}