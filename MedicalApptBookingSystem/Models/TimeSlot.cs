using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApptBookingSystem.Models
{
    public class TimeSlot
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
        public User Doctor { get; set; } = null!;

        public bool IsBooked { get; set; }
    }
}
