using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApptBookingSystem.Models
{
    public class TimeSlot
    {
        public int Id { get; set; }

        [Required]
        public required DateOnly Date { get; set; }

        [Required]
        public required TimeOnly StartTime { get; set; }

        [Required]
        public required TimeOnly EndTime { get; set; }

        [ForeignKey("Doctor")]
        public required int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        [Required]
        public required bool IsBooked { get; set; }
    }
}
