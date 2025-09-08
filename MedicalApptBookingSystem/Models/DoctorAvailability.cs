using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApptBookingSystem.Models
{
    public class DoctorAvailability
    {
        public int Id { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        // Day of the week
        public required DayOfWeek DayOfWeek { get; set; }

        // Time range doctor is available on that day
        public required TimeOnly StartTime { get; set; }
        public required TimeOnly EndTime { get; set; }

        // Date range for scheduling purposes
        public required DateOnly StartDate { get; set; }
        public required DateOnly EndDate { get; set; }




    }
}
