using MedicineDispencer.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register Compartments as a singleton
builder.Services.AddSingleton<CompartmentsData>();
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddSingleton<LogService>();
builder.Services.AddSingleton<CameraService>();

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

// Save CompartmentsData on application shutdown
var compartmentsData = app.Services.GetRequiredService<CompartmentsData>();

// Load CompartmentsData on application startup
var loadedData = CompartmentsData.LoadAsync().GetAwaiter().GetResult();
if (loadedData != null)
{
    // Copy loaded compartments into the singleton instance
    compartmentsData.compartments = loadedData.compartments;
}

app.Lifetime.ApplicationStopping.Register(() =>
{
    // Fire and forget async save
    compartmentsData.SaveAsync().GetAwaiter().GetResult();
});

app.Lifetime.ApplicationStopping.Register(() => LEDService.Dispose());
app.Run();