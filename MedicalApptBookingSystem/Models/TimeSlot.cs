using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApptBookingSystem.Models
{
    public class TimeSlot
    {
        public int Id { get; set; }

        [Required]
        public required DateTime StartTime { get; set; }

        [Required]
        public required DateTime EndTime { get; set; }

        [ForeignKey("Doctor")]
        public required int DoctorId { get; set; }
        public User Doctor { get; set; } = null!;

        public required bool IsBooked { get; set; }
    }
}
