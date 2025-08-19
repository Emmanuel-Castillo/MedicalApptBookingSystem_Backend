namespace MedicalApptBookingSystem.DTO.Reponses
{
    public class GetTimeSlotInfoResponse
    {
        public required TimeSlotDto TimeSlot { get; set; }
        public AppointmentDto? Appointment { get; set; }
    }
}
