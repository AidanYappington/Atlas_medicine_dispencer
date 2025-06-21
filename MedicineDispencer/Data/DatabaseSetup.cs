using Microsoft.EntityFrameworkCore;
using System;


namespace MedicineDispencer.Data;

public class Compartment
{
    public int Id { get; set; }

    // Foreign key to Medication
    public int? MedicationId { get; set; }
    public Medication? Medication { get; set; }

    public int Stock { get; set; }

    public List<DosingTime> DosingTimes { get; set; } = new();
}


public class Medication
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
}

public class RefillLog
{
    public int Id { get; set; }
    public int CompartmentId { get; set; }
    public DateTime RefillTime { get; set; }
    public int AmountAdded { get; set; }
}

public class Schedule
{
    public int Id { get; set; }
    public int CompartmentId { get; set; }
    public DateTime DispenseTime { get; set; }
    public int AmountToDispense { get; set; }
}

public class DosingTime
{
    public int Id { get; set; }
    public TimeSpan Time { get; set; }

    public int CompartmentId { get; set; }  // foreign key
    public Compartment Compartment { get; set; }
}

public class PillDispenserContext : DbContext
{
    public PillDispenserContext(DbContextOptions<PillDispenserContext> options)
        : base(options) { }

    public DbSet<Compartment> Compartments { get; set; }
    public DbSet<RefillLog> RefillLogs { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<DosingTime> DosingTimes { get; set; }
    public DbSet<Medication> Medications { get; set; }

}
