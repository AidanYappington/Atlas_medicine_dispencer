using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AtlasDispenserApp.Services
{
    public interface IMedicineJsonService
    {
        Task<string> ProcessMedicineJsonAsync(IFormFile jsonFile);
    }
}
