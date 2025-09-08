using MedicalApptBookingSystem.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApptBookingSystem.DTO
{
    public class AppointmentDto
    {

        public int Id { get; set; }

        public required int TimeSlotId { get; set; }
        public required TimeSlotDto TimeSlot { get; set; }
        public required int PatientId { get; set; }
        public required PatientDto Patient { get; set; }
        public required int DoctorId { get; set; }
        public required DoctorDto Doctor { get; set; }
        public string? Notes { get; set; }
    }
}
