using MedicalApptBookingSystem.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApptBookingSystem.DTO
{
    public class AppointmentDto
    {

        public int Id { get; set; }

        public int TimeSlotId { get; set; }
        public required TimeSlotDto TimeSlot { get; set; }

        public int PatientId { get; set; }
        public required UserDto Patient { get; set; }

        public string? Notes { get; set; }
    }
}
