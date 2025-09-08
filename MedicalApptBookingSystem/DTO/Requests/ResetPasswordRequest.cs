namespace MedicalApptBookingSystem.DTO.Requests
{
    public class ResetPasswordRequest
    {
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }
}
