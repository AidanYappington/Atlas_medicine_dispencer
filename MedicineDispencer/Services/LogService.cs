using System.Text.Json;

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string? MedicationName { get; set; }
    public string? Dose { get; set; }
    public int Compartment { get; set; }
}

public class LogService
{
    private readonly string logFilePath = Path.Combine("Data", "logs.json");

    public async Task AddLogAsync(LogEntry entry)
    {
        List<LogEntry> logs = new();
        if (File.Exists(logFilePath))
        {
            var json = await File.ReadAllTextAsync(logFilePath);
            if (!string.IsNullOrWhiteSpace(json))
                logs = JsonSerializer.Deserialize<List<LogEntry>>(json) ?? new();
        }
        logs.Add(entry);
        var updatedJson = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(logFilePath, updatedJson);
    }

    public async Task<List<LogEntry>> GetLogsAsync()
    {
        var logFilePath = Path.Combine("Data", "logs.json");
        if (!File.Exists(logFilePath))
            return new List<LogEntry>();

        var json = await File.ReadAllTextAsync(logFilePath);
        if (string.IsNullOrWhiteSpace(json))
            return new List<LogEntry>();

        return JsonSerializer.Deserialize<List<LogEntry>>(json) ?? new List<LogEntry>();
    }
}