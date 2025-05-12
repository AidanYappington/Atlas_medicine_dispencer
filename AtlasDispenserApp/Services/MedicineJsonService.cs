using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AtlasDispenserApp.Data;
using AtlasDispenserApp.Models;

namespace AtlasDispenserApp.Services
{
    public class MedicineJsonService : IMedicineJsonService
    {
        private readonly AppDbContext _dbContext;

        public MedicineJsonService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> ProcessMedicineJsonAsync(IFormFile jsonFile)
        {
            if (jsonFile == null || jsonFile.Length == 0)
                return "File is empty or null.";

            using var stream = new StreamReader(jsonFile.OpenReadStream());
            var content = await stream.ReadToEndAsync();

            MedicineData? medicine;
            try
            {
                medicine = JsonSerializer.Deserialize<MedicineData>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (medicine == null)
                    return "Invalid JSON structure.";

                if (string.IsNullOrWhiteSpace(medicine.Name) ||
                    string.IsNullOrWhiteSpace(medicine.Description) ||
                    medicine.AmountPerDose <= 0 ||
                    medicine.TotalAmount <= 0 ||
                    medicine.IntervalHours <= 0)
                {
                    return "Missing or invalid fields in JSON.";
                }

                var medicineFile = new MedicineFile
                {
                    FileName = jsonFile.FileName,
                    Content = content,
                    UploadedAt = DateTime.UtcNow,
                    Name = medicine.Name,
                    Description = medicine.Description,
                    AmountPerDose = medicine.AmountPerDose,
                    TotalAmount = medicine.TotalAmount,
                    IntervalHours = medicine.IntervalHours
                };

                _dbContext.MedicineFiles.Add(medicineFile);
                await _dbContext.SaveChangesAsync();

                return "JSON successfully processed and stored.";
            }
            catch (JsonException)
            {
                return "Error parsing JSON.";
            }
        }
    }

    public class MedicineData
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int AmountPerDose { get; set; }
        public int TotalAmount { get; set; }
        public int IntervalHours { get; set; }
    }
}
