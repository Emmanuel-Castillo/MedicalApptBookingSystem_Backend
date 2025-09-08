using MedicalApptBookingSystem.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApptBookingSystem.DTO
{
    public class PatientDto
    {
        public required int Id { get; set; }
        public required float HeightImperial { get; set; }
        public required float WeightImperial { get; set; }
        public required int UserId { get; set; }
        public required UserDto User { get; set; }
    }
}
