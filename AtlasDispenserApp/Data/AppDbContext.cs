using Microsoft.EntityFrameworkCore;
using AtlasDispenserApp.Models;
using System.IO;


namespace AtlasDispenserApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Medicine> Medicine { get; set; }

    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<Compartiment1> Compartiment1 => Set<Compartiment1>();
    public DbSet<Compartiment2> Compartiment2 => Set<Compartiment2>();
    public DbSet<Compartiment3> Compartiment3 => Set<Compartiment3>();
    public DbSet<Compartiment4> Compartiment4 => Set<Compartiment4>();

}
