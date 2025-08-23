namespace MedicalApptBookingSystem.DTO.Reponses
{
    public class GetPatientAppointmentsResponse
    {
        public required List<AppointmentDto> Appointments { get; set; }

        public required int TotalCount { get; set; }
    }
}
