using MedicineDispencer.Components;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMudServices();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register Compartments as a singleton
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


app.Lifetime.ApplicationStopping.Register(() =>
{
    // Fire and forget async save
    DataService.SaveCompartmentsAsync().GetAwaiter().GetResult();
});

app.Lifetime.ApplicationStopping.Register(() => LEDService.Dispose());
app.Lifetime.ApplicationStopping.Register(() => ServoService.Dispose());
app.Lifetime.ApplicationStopping.Register(() => ButtonService.Dispose());
await DataService.LoadCompartmentsAsync();

app.Run();