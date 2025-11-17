using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApptBookingSystem.Models
{
    public class Patient
    {
        public int Id { get; set; }

        public required float HeightImperial { get; set; }

        public required float WeightImperial { get; set; }

        [ForeignKey("User")]
        public required int UserId { get; set; }
        public User User { get; set; } = null!;

        // Navigation to their booked appointments
        public ICollection<Appointment> Appointments { get; set; } = [];
    }
}
