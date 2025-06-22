using System.Text.Json;
using System.Text.Json.Serialization;

public static class DataService
{
    private static readonly string compartmentsFilePath = Path.Combine("Data", "savedData.json");

    public static async Task SaveCompartmentsAsync()
    {
        var dir = Path.GetDirectoryName(compartmentsFilePath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir!);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        var json = JsonSerializer.Serialize(CompartmentsData.compartments, options);
        await File.WriteAllTextAsync(compartmentsFilePath, json);
    }

    public static async Task LoadCompartmentsAsync()
    {
        if (!File.Exists(compartmentsFilePath))
            return;

        var json = await File.ReadAllTextAsync(compartmentsFilePath);
        if (string.IsNullOrWhiteSpace(json))
            return;

        var loadedDtos = JsonSerializer.Deserialize<MedicijnCompartimentDto?[]>(json);
        if (loadedDtos == null)
            return;

        CompartmentsData.compartments = loadedDtos
            .Select(dto => dto == null ? null :
                new MedicijnCompartiment(
                    dto.MedicijnNaam,
                    dto.Dosis,
                    dto.Voorraad,
                    dto.DoseringstijdenPerDag.Select(TimeSpan.Parse).ToList(),
                    (CompartimentStatus)dto.Status,
                    dto.LaatsteOpeningTijd
                )
            ).ToArray();
    }

    public static bool AddToFirstEmpty(MedicijnCompartiment compartment)
    {
        for (int i = 0; i < CompartmentsData.compartments.Length; i++)
        {
            if (CompartmentsData.compartments[i] == null)
            {
                CompartmentsData.compartments[i] = compartment;
                return true;
            }
        }
        return false; // No empty compartment found
    }
}

public class MedicijnCompartimentDto
{
    public string MedicijnNaam { get; set; }
    public string Dosis { get; set; }
    public int Status { get; set; }
    public int Voorraad { get; set; }
    public List<string> DoseringstijdenPerDag { get; set; }
    public DateTime? LaatsteOpeningTijd { get; set; }
}