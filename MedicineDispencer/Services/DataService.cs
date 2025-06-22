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

    public static async Task<MedicijnCompartiment?[]?> LoadCompartmentsAsync(int expectedLength)
    {
        if (!File.Exists(compartmentsFilePath))
            return null;

        var json = await File.ReadAllTextAsync(compartmentsFilePath);
        if (string.IsNullOrWhiteSpace(json))
            return null;

        var loadedCompartments = JsonSerializer.Deserialize<MedicijnCompartiment?[]>(json);
        if (loadedCompartments == null)
            return null;

        // Ensure the array is the expected length
        if (loadedCompartments.Length < expectedLength)
        {
            var resized = new MedicijnCompartiment?[expectedLength];
            loadedCompartments.CopyTo(resized, 0);
            return resized;
        }
        return loadedCompartments;
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