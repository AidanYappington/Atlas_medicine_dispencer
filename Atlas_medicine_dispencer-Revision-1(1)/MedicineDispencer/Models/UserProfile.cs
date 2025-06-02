namespace AtlasDispenserApp.Models;

using System.ComponentModel.DataAnnotations;

public class UserProfile
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

