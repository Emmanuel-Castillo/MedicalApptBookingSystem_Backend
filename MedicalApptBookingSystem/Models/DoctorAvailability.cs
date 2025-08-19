using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApptBookingSystem.Models
{
    public class DoctorAvailability
    {
        public int Id { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
        public User Doctor { get; set; } = null!;

        // Day of the week
        public required DayOfWeek DayOfWeek { get; set; }

        // Time range doctor is available on that day
        public required TimeSpan StartTime { get; set; }
        public required TimeSpan EndTime { get; set; }

        // Date range for scheduling purposes
        // EndDate can be optional if the doctor is available on that day forever
        public required DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }




    }
}
