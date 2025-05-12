namespace AtlasDispenserApp.Models;

public class MedicineFile
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int AmountPerDose { get; set; }
    public int TotalAmount { get; set; }
    public int IntervalHours { get; set; }
}
