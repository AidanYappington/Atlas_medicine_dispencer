using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AtlasDispenserApp.Models;

public class Compartiment4
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Medicine")]
    public int MedicineId { get; set; }

    public int InStock { get; set; }

    public Medicine? Medicine { get; set; }
}
