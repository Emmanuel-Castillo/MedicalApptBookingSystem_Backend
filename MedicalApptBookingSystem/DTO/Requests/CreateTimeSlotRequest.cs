namespace MedicalApptBookingSystem.DTO.Requests
{
    public class CreateTimeSlotRequest
    {
        public string? DoctorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
