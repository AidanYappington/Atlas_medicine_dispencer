using Microsoft.EntityFrameworkCore;
using System;


namespace MedicineDispencer.Data;

public class Compartment
{
    public int Id { get; set; }
    
    public int CompartmentNumber { get; set; }

    public int? MedicationId { get; set; }
    public MedicationStock? Medication { get; set; }

    public int Stock { get; set; }

    public List<DosingTime> DosingTimes { get; set; } = new();
}



public class MedicationStock
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;

    public int Stock { get; set; } // ← THIS is important!
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
    public int IntervalSeconds { get; set; }

    public int CompartmentId { get; set; }
    public Compartment Compartment { get; set; }
}



public class PillDispenserContext : DbContext
{
    public PillDispenserContext(DbContextOptions<PillDispenserContext> options)
        : base(options) { }

    public DbSet<MedicationStock> Medications { get; set; }
    public DbSet<Compartment> Compartments { get; set; }
    public DbSet<DosingTime> DosingTimes { get; set; }
    public DbSet<RefillLog> RefillLogs { get; set; }
    public DbSet<Schedule> Schedules { get; set; }

}
