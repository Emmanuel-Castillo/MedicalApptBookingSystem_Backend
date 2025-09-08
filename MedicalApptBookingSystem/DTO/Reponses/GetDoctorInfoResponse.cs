namespace MedicalApptBookingSystem.DTO.Reponses
{
    public class GetDoctorInfoResponse
    {
        public required DoctorDto DoctorProfile { get; set; }
        public required List<TimeSlotDto> BookedTimeSlotsNextTwoWeeks { get; set; }

    }
}
