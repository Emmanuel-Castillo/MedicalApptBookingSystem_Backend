namespace MedicalApptBookingSystem.DTO.Requests
{
    public class ChangeUserRequest
    {
        public required int Id { get; set; }
        public required string NewFullName { get; set; }

        public required string NewEmail { get; set; }
    }
}
