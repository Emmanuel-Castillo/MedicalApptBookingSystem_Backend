namespace MedicalApptBookingSystem.Models
{

    public enum UserRole
    {
        Patient,
        Doctor,
        Admin
    }
    public class User
    {
        public int Id { get; set; }
        public required string FullName { get; set; } = string.Empty;
        public required string Email { get; set; } = string.Empty;
        public required string PasswordHash { get; set; } = string.Empty;
        public required UserRole Role { get; set; }
    }
}
