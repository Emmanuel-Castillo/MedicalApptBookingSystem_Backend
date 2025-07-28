using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApptBookingSystem.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [ForeignKey("TimeSlot")]
        public int TimeSlotId { get; set; }
        public TimeSlot TimeSlot { get; set; } = null!;

        [ForeignKey("Patient")]
        public int PatientId { get; set; }
        public User Patient { get; set; } = null!;

        public string? Notes { get; set; }
    }
}
