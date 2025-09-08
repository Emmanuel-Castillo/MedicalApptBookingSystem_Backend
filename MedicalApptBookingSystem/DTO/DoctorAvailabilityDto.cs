using MedicalApptBookingSystem.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApptBookingSystem.DTO
{
    public class DoctorAvailabilityDto
    {
        public int Id { get; set; }

        public required DoctorDto Doctor { get; set; }
        public required string DayOfWeek { get; set; }
        public required TimeOnly StartTime { get; set; }
        public required TimeOnly EndTime { get; set; }
        public required DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }
}
