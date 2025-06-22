using System.Text.Json;
using System.Text.Json.Serialization;

public class CompartmentsData
{
    public MedicijnCompartiment?[] compartments = [
        new MedicijnCompartiment("Paracetamol", "20g", 15, new List<TimeSpan> { new TimeSpan(8, 0, 0) }),
        new MedicijnCompartiment("Vitamine D", "20mcg", 20, new List<TimeSpan> { new TimeSpan(8, 0, 0) }),
        null,
        null
    ];

    private static readonly string compartmentsFilePath = Path.Combine("Data", "savedData.json");

    public async Task SaveAsync()
    {
        var dir = Path.GetDirectoryName(compartmentsFilePath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir!);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        var json = JsonSerializer.Serialize(compartments, options);
        await File.WriteAllTextAsync(compartmentsFilePath, json);
    }

    public static async Task<CompartmentsData?> LoadAsync()
    {
        if (!File.Exists(compartmentsFilePath))
            return null;

        var json = await File.ReadAllTextAsync(compartmentsFilePath);
        if (string.IsNullOrWhiteSpace(json))
            return null;

        // Define a DTO for deserialization
        var loadedCompartments = JsonSerializer.Deserialize<SimpleMedicijnCompartiment?[]>(json);
        if (loadedCompartments == null)
            return null;

        // Map DTOs to real MedicijnCompartiment objects
        var compartments = loadedCompartments
            .Select(s => s == null ? null :
                new MedicijnCompartiment(
                    s.MedicijnNaam,
                    s.Dosis,
                    s.Voorraad,
                    s.DoseringstijdenPerDag
                ))
            .ToArray();

        return new CompartmentsData { compartments = compartments };
    }

    // Helper DTO for deserialization
    public class SimpleMedicijnCompartiment
    {
        public string MedicijnNaam { get; set; }
        public string Dosis { get; set; }
        public int Voorraad { get; set; }
        public List<TimeSpan> DoseringstijdenPerDag { get; set; }
    }

    public bool AddToFirstEmpty(MedicijnCompartiment newCompartment)
    {
        for (int i = 0; i < compartments.Length; i++)
        {
            if (compartments[i] == null)
            {
                compartments[i] = newCompartment;
                return true;
            }
        }
        return false; // No empty slot found
    }
}