namespace AtlasDispenserApp.Models;

using System.ComponentModel.DataAnnotations;

public class Medicine
{
    [Key]
    public int Id { get; set; }

    public string Barcode { get; set; } = string.Empty; // Gebruik string voor barcodes
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AmountPerDose { get; set; } = string.Empty;
    public string IntervalHours { get; set; } = string.Empty;
    public int TotalAmount { get; set; }
}
