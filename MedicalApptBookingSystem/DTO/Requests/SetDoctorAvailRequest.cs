namespace MedicalApptBookingSystem.DTO.Requests
{
    public class SetDoctorAvailRequest
    {
        public required int DoctorId { get; set; }

        public required List<DayOfWeek> DaysOfWeek { get; set; }

        public required TimeSpan StartTime { get; set; }

        public required TimeSpan EndTime { get; set; }

        public required DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
