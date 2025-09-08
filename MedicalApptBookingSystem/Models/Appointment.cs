using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApptBookingSystem.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [ForeignKey("TimeSlot")]
        public required int TimeSlotId { get; set; }
        public TimeSlot TimeSlot { get; set; } = null!;

        [ForeignKey("Patient")]
        public required int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        [ForeignKey("Doctor")]
        public required int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public string? Notes { get; set; }
    }
}
