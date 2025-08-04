namespace MedicalApptBookingSystem.DTO.Reponses
{
    public class GetDoctorInfoResponse
    {
        public required UserDto Doctor { get; set; }
        public required List<TimeSlotDto> TimeSlots { get; set; }

    }
}
