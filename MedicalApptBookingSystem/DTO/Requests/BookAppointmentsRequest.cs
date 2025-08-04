namespace MedicalApptBookingSystem.DTO.Requests
{
    public class BookAppointmentsRequest
    {
        public string? PatientId { get; set; }
        public int TimeSlotId { get; set; }
    }
}
