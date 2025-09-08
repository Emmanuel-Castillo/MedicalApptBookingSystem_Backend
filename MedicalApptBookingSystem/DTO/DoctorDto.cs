namespace MedicalApptBookingSystem.DTO
{
    public class DoctorDto
    {
        public required int Id { get; set; }
        public required string Specialty { get; set; }
        public required int UserId { get; set; }
        public required UserDto User { get; set; }

    }
}
