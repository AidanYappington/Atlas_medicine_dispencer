using Microsoft.EntityFrameworkCore;
using AtlasDispenserApp.Models;
using System.IO;


namespace AtlasDispenserApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<MedicineFile> MedicineFiles => Set<MedicineFile>();
}
