namespace MedicalApptBookingSystem.DTO.Requests
{
    public class CreateTimeSlotRequest
    {
        public string DoctorId { get; set; }

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
