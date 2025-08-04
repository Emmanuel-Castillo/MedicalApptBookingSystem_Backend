namespace MedicalApptBookingSystem.DTO.Reponses
{
    public class GetPatientInfoResponse
   {
        public required UserDto Patient { get; set; }
        public required List<AppointmentDto> Appointments { get; set; }

    }
}
