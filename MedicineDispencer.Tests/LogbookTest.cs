using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using System.Linq;

public class LogbookTests
{
    [Fact]
    public async Task LoadLogs_ShouldRetrieveLogsCorrectly()
    {
        // Arrange
        var mockService = new Mock<LogService>();
        var fakeLogs = new List<LogEntry>
        {
            new LogEntry { Timestamp = DateTime.Now, Compartment = 1, MedicationName = "TestMed", Dose = "500mg" },
            new LogEntry { Timestamp = DateTime.Now, Compartment = 2, MedicationName = "AnotherMed", Dose = "100mg" }
        };
        mockService.Setup(s => s.GetLogsAsync()).ReturnsAsync(fakeLogs);

        var component = new TestableLogbook(mockService.Object);

        // Act
        await component.LoadLogsAsync();

        // Assert
        Assert.Equal(2, component.Logs.Count);
    }

    [Fact]
    public void Pagination_WorksCorrectly()
    {
        // Arrange
        var component = new TestableLogbook(null!);
        for (int i = 0; i < 30; i++)
        {
            component.Logs.Add(new LogEntry { Timestamp = DateTime.Now, Compartment = i, MedicationName = $"Med{i}", Dose = "10mg" });
        }

        component.PageSize = 10;
        component.CurrentPage = 2;

        // Act
        var page = component.PagedLogs.ToList();

        // Assert
        Assert.Equal(10, page.Count);
        Assert.Equal("Med10", page[0].MedicationName);
    }

    [Fact]
    public void ExportLogs_ShouldGenerateCsvString()
    {
        // Arrange
        var logs = new List<LogEntry>
        {
            new LogEntry { Timestamp = new DateTime(2025, 6, 30, 10, 0, 0), Compartment = 1, MedicationName = "Paracetamol", Dose = "500mg" },
            new LogEntry { Timestamp = new DateTime(2025, 6, 30, 12, 0, 0), Compartment = 2, MedicationName = "Ibuprofen", Dose = "200mg" },
        };

        var component = new TestableLogbook(null!) { Logs = logs };

        // Act
        var csv = component.GenerateCsv();

        // Assert
        Assert.Contains("Paracetamol", csv);
        Assert.Contains("Ibuprofen", csv);
        Assert.Contains("Tijd;Compartiment;Medicijn;Dosis", csv);
    }

    private class TestableLogbook
    {
        public List<LogEntry> Logs { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        private readonly LogService? _logService;

        public TestableLogbook(LogService? logService)
        {
            _logService = logService;
        }

        public IEnumerable<LogEntry> PagedLogs =>
            Logs.Skip((CurrentPage - 1) * PageSize).Take(PageSize);

        public async Task LoadLogsAsync()
        {
            if (_logService != null)
                Logs = await _logService.GetLogsAsync();
        }

        public string GenerateCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Tijd;Compartiment;Medicijn;Dosis");
            foreach (var log in Logs)
            {
                sb.AppendLine($"\"{log.Timestamp:dd-MM-yyyy HH:mm}\";\"{log.Compartment}\";\"{log.MedicationName}\";\"{log.Dose}\"");
            }
            return sb.ToString();
        }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string? MedicationName { get; set; }
        public string? Dose { get; set; }
        public int Compartment { get; set; }
    }

    public abstract class LogService
    {
        public abstract Task<List<LogEntry>> GetLogsAsync();
    }
}
