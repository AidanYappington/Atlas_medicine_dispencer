namespace AtlasDispenserApp.Models
{
    public class Compartment
    {
        public int Id { get; set; }
        public int CompartmentNumber { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public int CurrentStock { get; set; } = 0;
        public string Status { get; set; } = "locked"; // "locked", "unlocked", "open"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public List<DosingSchedule> DosingSchedules { get; set; } = new List<DosingSchedule>();
    }

    public class DosingSchedule
    {
        public int Id { get; set; }
        public int CompartmentId { get; set; }
        public TimeSpan TimeOfDay { get; set; }
        public int? DayOfWeek { get; set; } // 0-6 for Sunday-Saturday (null means every day)
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class MedicationLog
    {
        public int Id { get; set; }
        public int CompartmentId { get; set; }
        public DateTime TakenAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = string.Empty; // "taken", "missed", "reminded"
        public string? Notes { get; set; }
        public int? StockAfter { get; set; } // Tracks remaining stock after this dose
    }

    public class RefillHistory
    {
        public int Id { get; set; }
        public int CompartmentId { get; set; }
        public DateTime RefilledAt { get; set; } = DateTime.UtcNow;
        public int AmountAdded { get; set; }
        public int PreviousStock { get; set; }
        public int NewStock { get; set; }
        public string? PerformedBy { get; set; } // Optional: track who did the refill
    }

    public class EmergencyUnlock
    {
        public int Id { get; set; }
        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
        public string? UnlockedBy { get; set; } // Optional: track who triggered it
        public string? Reason { get; set; }
    }

    public class SystemSetting
    {
        public int Id { get; set; }
        public string SettingKey { get; set; } = string.Empty;
        public string? SettingValue { get; set; }
        public string? Description { get; set; }
    }
}