namespace MedicalApptBookingSystem.DTO.Reponses
{
    public class GetPatientInfoResponse
   {
        public required PatientDto PatientProfile { get; set; }
        public required List<AppointmentDto> AppointmentsThisWeek { get; set; }

    }
}
