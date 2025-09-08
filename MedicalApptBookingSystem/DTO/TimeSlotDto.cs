namespace MedicalApptBookingSystem.DTO
{
    public class TimeSlotDto
    {
        public required int Id { get; set; }
        public required DateOnly Date { get; set; }
        public required TimeOnly StartTime { get; set; }
        public required TimeOnly EndTime { get; set; }
        public required bool IsBooked { get; set; }
        public required DoctorDto Doctor { get; set; }
    }
}
