namespace MedicalApptBookingSystem.DTO
{
    public class TimeSlotDto
    {
        public required int Id { get; set; }
        public required DateTime StartTime { get; set; }
        public required DateTime EndTime { get; set; }
        public required bool IsBooked { get; set; }
        public required UserDto Doctor { get; set; }
    }
}
