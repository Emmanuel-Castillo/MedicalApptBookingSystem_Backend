namespace MedicalApptBookingSystem.DTO.Requests
{
    public class SetDoctorAvailRequest
    {
        public required int DoctorId { get; set; }

        public required List<DayOfWeek> DaysOfWeek { get; set; }

        public required TimeOnly StartTime { get; set; }

        public required TimeOnly EndTime { get; set; }

        public required DateOnly StartDate { get; set; }

        public required DateOnly EndDate { get; set; }
    }
}
