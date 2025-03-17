using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BlazorApp1.Data;

namespace BlazorApp1.Services
{
    public class MedicationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MedicationService> _logger;

        public MedicationService(ApplicationDbContext context, ILogger<MedicationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Medication>> GetMedicationsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching medications from the database...");
                var meds = await _context.Medications.ToListAsync();
                _logger.LogInformation($"Fetched {meds.Count} medications.");
                return meds;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching medications: {ex.Message}");
                return new List<Medication>();
            }
        }
    }
}
