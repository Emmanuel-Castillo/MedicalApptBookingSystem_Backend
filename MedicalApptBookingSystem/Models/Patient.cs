using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApptBookingSystem.Models
{
    public class Patient
    {
        public required int Id { get; set; }

        public required float HeightImperial { get; set; }

        public required float WeightImperial { get; set; }

        [ForeignKey("User")]
        public required int UserId { get; set; }
        public required User User { get; set; }

        // Navigation to their booked appointments
        public ICollection<Appointment> Appointments { get; set; } = [];
    }
}
