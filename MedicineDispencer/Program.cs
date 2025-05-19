using MedicineDispencer.Components;
using System.Timers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add a service to initialize default data
builder.Services.AddSingleton<IDataInitializer, DataInitializer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

// Initialize defaults on application startup
using (var scope = app.Services.CreateScope())
{
    var dataInitializer = scope.ServiceProvider.GetRequiredService<IDataInitializer>();
    dataInitializer.InitializeDefaults();
}

app.Run();

// Example of a data initializer service
public interface IDataInitializer
{
    void InitializeDefaults();
}

public class DataInitializer : IDataInitializer
{
    private List<MedicijnCompartiment?> compartments = new List<MedicijnCompartiment?>();
    private System.Timers.Timer checkMedicationsTimer;

    public void InitializeDefaults()
    {
        // Initialize with 4 empty compartments
        for (int i = 0; i < 4; i++)
            compartments.Add(null);

        // Check medication schedules every minute
        checkMedicationsTimer = new System.Timers.Timer(60000);
        checkMedicationsTimer.Elapsed += CheckMedicationsTimerElapsed;
        checkMedicationsTimer.AutoReset = true;
        checkMedicationsTimer.Start();

        // Initialize GPIO
        InitializeGpio();

        // Setup example
        SetupVoorbeeldDispenser();
    }

    private void CheckMedicationsTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        // Logic to check medication schedules
    }

    private void InitializeGpio()
    {
        // Logic to initialize GPIO
    }

    private void SetupVoorbeeldDispenser()
    {
        // Logic to set up example dispenser
    }
}
