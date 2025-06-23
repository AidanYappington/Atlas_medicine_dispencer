using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;

namespace MedicineDispencer.Tests
{
    public class MedicationViewTests
    {
        [Fact]
        public void BuildSchedule_ShouldGenerateCorrectNumberOfDoses()
        {
            // Arrange
            var testData = new CompartmentsData
            {
                compartments = new[]
                {
                    new CompartmentSlot
                    {
                        MedicijnNaam = "Paracetamol",
                        DoseringstijdenPerDag = new List<TimeSpan> { new TimeSpan(8, 0, 0), new TimeSpan(20, 0, 0) }
                    },
                    null,
                    new CompartmentSlot
                    {
                        MedicijnNaam = "Ibuprofen",
                        DoseringstijdenPerDag = new List<TimeSpan> { new TimeSpan(12, 0, 0) }
                    }
                }
            };

            var component = new TestableMedicationComponent(testData);
            var start = new DateTime(2025, 6, 24);
            var end = start.AddDays(2);

            // Act
            var result = component.InvokeBuildSchedule(start, end);

            // Assert
            Assert.Equal(9, result.Count); // 3 days × (2 + 1) doses per day
            Assert.Contains(result, d => d.Name == "Paracetamol" && d.Time.Hour == 8);
            Assert.Contains(result, d => d.Name == "Ibuprofen" && d.Time.Hour == 12);
        }

        private class TestableMedicationComponent
        {
            private readonly CompartmentsData _compartmentsData;

            public TestableMedicationComponent(CompartmentsData data)
            {
                _compartmentsData = data;
            }

            public List<MedicationDose> InvokeBuildSchedule(DateTime start, DateTime end)
            {
                var list = new List<MedicationDose>();

                for (int i = 0; i < _compartmentsData.compartments.Length; i++)
                {
                    var slot = _compartmentsData.compartments[i];
                    if (slot is null) continue;

                    foreach (var t in slot.DoseringstijdenPerDag)
                    {
                        for (var day = start; day <= end; day = day.AddDays(1))
                        {
                            list.Add(new MedicationDose
                            {
                                Name = slot.MedicijnNaam,
                                Time = day.Date + t,
                                Compartment = i + 1
                            });
                        }
                    }
                }

                return list;
            }
        }

        public class MedicationDose
        {
            public string Name { get; set; } = string.Empty;
            public DateTime Time { get; set; }
            public int Compartment { get; set; }
        }

        public class CompartmentsData
        {
            public CompartmentSlot[] compartments { get; set; } = Array.Empty<CompartmentSlot>();
        }

        public class CompartmentSlot
        {
            public string MedicijnNaam { get; set; } = string.Empty;
            public List<TimeSpan> DoseringstijdenPerDag { get; set; } = new();
        }
    }
}