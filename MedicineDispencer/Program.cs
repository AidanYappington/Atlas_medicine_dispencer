using MedicineDispencer.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using System.Timers;


var builder = WebApplication.CreateBuilder(args);

// ✅ Register services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// ✅ Add SQLite DB context
builder.Services.AddDbContext<PillDispenserContext>(options =>
    options.UseSqlite("Data Source=pilldispenser.db"));

// ✅ Add your data initializer
builder.Services.AddSingleton<IDataInitializer, DataInitializer>();

var app = builder.Build();

// ✅ Ensure DB is created + seed default data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PillDispenserContext>();
    db.Database.EnsureCreated();

    var dataInitializer = scope.ServiceProvider.GetRequiredService<IDataInitializer>();
    dataInitializer.InitializeDefaults();
}

// ✅ Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

public interface IDataInitializer
{
    void InitializeDefaults();
}

public class DataInitializer : IDataInitializer
{
    private readonly IServiceProvider _services;
    private List<Compartment?> compartments = new();

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
