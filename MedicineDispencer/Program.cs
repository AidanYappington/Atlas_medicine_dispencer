using MedicineDispencer.Components;
using Microsoft.EntityFrameworkCore;
using MedicineDispencer.Data;
using MedicineDispencer;


using System.Timers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register the database context BEFORE building
builder.Services.AddDbContext<PillDispenserContext>(options =>
    options.UseSqlite("Data Source=pilldispenser.db"));

// Register the default data initializer
builder.Services.AddSingleton<IDataInitializer, DataInitializer>();

var app = builder.Build();

// Ensure the database is created on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PillDispenserContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Run any startup logic (data init, timers, GPIO)
using (var scope = app.Services.CreateScope())
{
    var dataInitializer = scope.ServiceProvider.GetRequiredService<IDataInitializer>();
    dataInitializer.InitializeDefaults();
}

app.Run();

// -----------------------------
// Supporting Services
// -----------------------------

public interface IDataInitializer
{
    void InitializeDefaults();
}

public class DataInitializer : IDataInitializer
{
    private List<MedicijnCompartiment?> compartments = new();
    private System.Timers.Timer? checkMedicationsTimer;


    public void InitializeDefaults()
    {
        // Example: Initialize 4 empty compartments
        for (int i = 0; i < 4; i++)
            compartments.Add(null);

        // Set up medication schedule checker every 60 sec
        checkMedicationsTimer = new System.Timers.Timer(60000);
        checkMedicationsTimer.Elapsed += CheckMedicationsTimerElapsed;
        checkMedicationsTimer.AutoReset = true;
        checkMedicationsTimer.Start();

        InitializeGpio();
        SetupVoorbeeldDispenser();
    }

    private void CheckMedicationsTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        // Medication schedule logic here
    }

    private void InitializeGpio()
    {
        // GPIO logic here
    }

    private void SetupVoorbeeldDispenser()
    {
        // Optional: preload a test configuration
    }
}
