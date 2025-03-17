using BlazorApp1.Components;
using BlazorApp1.Data;
using BlazorApp1.Hubs;
using BlazorApp1.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Database Setup
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=medications.db"));

// Register Services
builder.Services.AddScoped<MedicationService>();
builder.Services.AddTransient<ReminderService>();

// SignalR
builder.Services.AddSignalR();

// Quartz.NET for scheduled notifications
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("MedicationReminderJob");
    q.AddJob<ReminderService>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("MedicationReminderTrigger")
        .StartNow()
        .WithSimpleSchedule(x => x.WithIntervalInMinutes(1).RepeatForever()));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Add Blazor Services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Ensure Database and Seed Data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate(); // Apply migrations if not applied
    SeedDatabase(dbContext); // Insert dummy data
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

// Mapping
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();

// Method to Seed Dummy Medications
void SeedDatabase(ApplicationDbContext dbContext)
{
    if (!dbContext.Medications.Any()) // Check if there are no medications
    {
        dbContext.Medications.AddRange(new List<Medication>
        {
            new Medication { Name = "askdjsd", Dosage = "100mg", Time = DateTime.Now.AddMinutes(0.5) },
            new Medication { Name = "ayamen", Dosage = "100mg", Time = DateTime.Now.AddMinutes(0.5) },
            new Medication { Name = "Paracetamol", Dosage = "500mg", Time = DateTime.Now.AddHours(4) },
            new Medication { Name = "Ibuprofen", Dosage = "200mg", Time = DateTime.Now.AddHours(6) },
            new Medication { Name = "Amoxicillin", Dosage = "250mg", Time = DateTime.Now.AddHours(8) },
            new Medication { Name = "Metformin", Dosage = "850mg", Time = DateTime.Now.AddHours(10) }
        });

        dbContext.SaveChanges();
        Console.WriteLine("✅ Dummy medications inserted into the database!");
    }
    else
    {
        Console.WriteLine("⚠ Database already contains medications, skipping seeding.");
    }
}
