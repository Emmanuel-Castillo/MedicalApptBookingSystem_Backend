using MedicalApptBookingSystem.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApptBookingSystem.DTO
{
    public class DoctorAvailabilityDto
    {
        public int Id { get; set; }

        public required UserDto Doctor { get; set; }

        public required string DayOfWeek { get; set; }

        public required TimeSpan StartTime { get; set; }
        public required TimeSpan EndTime { get; set; }

        public required DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
