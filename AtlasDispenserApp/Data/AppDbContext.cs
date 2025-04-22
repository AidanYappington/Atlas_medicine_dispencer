using Microsoft.EntityFrameworkCore;
using AtlasDispenserApp.Models; // 👈 Needed for JsonFile

namespace AtlasDispenserApp.Data
{
    public class AppDbContext  : DbContext
    {
        public AppDbContext (DbContextOptions<AppDbContext > options)
            : base(options)
        {
        }

    public DbSet<JsonFile> JsonFiles { get; set; } // Ensure this is here
    }
}
