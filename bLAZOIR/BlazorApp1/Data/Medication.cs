using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Data
{
    public class Medication
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Dosage { get; set; }
        public DateTime Time { get; set; }
    }
}
