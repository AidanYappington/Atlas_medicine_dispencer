namespace AtlasDispenserApp.Models;

using System.ComponentModel.DataAnnotations;

public class UserProfile
{
    [Key]
    public int Id { get; set; }

    public string Naam { get; set; } = string.Empty;
    public int Leeftijd { get; set; }
}

