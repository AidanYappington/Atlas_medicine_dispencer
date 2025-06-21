using MedicineDispencer.Components;
using Microsoft.EntityFrameworkCore;
using MedicineDispencer.Data;
using MedicineDispencer;
using System.Timers;

var builder = WebApplication.CreateBuilder(args);

// ✅ Enable debug logging (optional)
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// ✅ Add Razor & Interactive components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ✅ Register the database context
builder.Services.AddDbContext<PillDispenserContext>(options =>
    options.UseSqlite("Data Source=pilldispenser.db"));

// ✅ Register DataInitializer with IServiceProvider injection
builder.Services.AddSingleton<IDataInitializer, DataInitializer>();

var app = builder.Build();

// ✅ Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PillDispenserContext>();
    db.Database.EnsureCreated();
}

// ✅ Configure middleware
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

// ✅ Run startup logic
using (var scope = app.Services.CreateScope())
{
    var dataInitializer = scope.ServiceProvider.GetRequiredService<IDataInitializer>();
    dataInitializer.InitializeDefaults();
}

app.Run();

// -----------------------------
// Startup logic service
// -----------------------------

public interface IDataInitializer
{
    void InitializeDefaults();
}

public class DataInitializer : IDataInitializer
{
    private readonly IServiceProvider _services;
    private List<MedicijnCompartiment?> compartments = new();
    private System.Timers.Timer? checkMedicationsTimer;

    public DataInitializer(IServiceProvider services)
    {
        _services = services;
    }

    public void InitializeDefaults()
    {
        // Init 4 empty compartments
        for (int i = 0; i < 4; i++)
            compartments.Add(null);

        checkMedicationsTimer = new System.Timers.Timer(60000);
        checkMedicationsTimer.Elapsed += CheckMedicationsTimerElapsed;
        checkMedicationsTimer.AutoReset = true;
        checkMedicationsTimer.Start();

        InitializeGpio();

        SeedDefaultMedications(); // ✅ Seed meds into DB
        SetupVoorbeeldDispenser();
    }

    private void SeedDefaultMedications()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PillDispenserContext>();

        if (!db.Medications.Any())
        {
            var meds = new List<MedicationStock>
            {
                new MedicationStock { Name = "Paracetamol", Dosage = "500mg", Stock = 30, Description = "Pijnstiller" },
                new MedicationStock { Name = "Ibuprofen", Dosage = "200mg", Stock = 20, Description = "Ontstekingsremmer" },
                new MedicationStock { Name = "Aspirine", Dosage = "100mg", Stock = 25, Description = "Bloedverdunner" },
                new MedicationStock { Name = "Omeprazol", Dosage = "20mg", Stock = 15, Description = "Maagzuurremmer" },
                new MedicationStock { Name = "Loratadine", Dosage = "10mg", Stock = 10, Description = "Allergiemedicatie" },
                new MedicationStock { Name = "Metformine", Dosage = "850mg", Stock = 40, Description = "Diabetesmedicatie" }
            };

            db.Medications.AddRange(meds);
            db.SaveChanges();
            Console.WriteLine("✅ Default medications seeded into the database.");
        }
        else
        {
            Console.WriteLine("ℹ️ Medications already exist — skipping seed.");
        }
    }

    private void CheckMedicationsTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        // TODO: Check medication logic
    }

    private void InitializeGpio()
    {
        // TODO: GPIO setup
    }

    private void SetupVoorbeeldDispenser()
    {
        // TODO: Preload compartments or settings
    }
}
