using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApptBookingSystem.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string Specialty { get; set; } = "General Physician";

        [ForeignKey("User")]
        public required int UserId { get; set; }
        public User User { get; set; } = null!;

        // Navigation to their availabilities, time slots, and appointments scheduled by patients
        public ICollection<DoctorAvailability> Availabilities { get; set; } = [];

        public ICollection<TimeSlot> TimeSlots { get; set; } = [];

        public ICollection<Appointment> Appointments { get; set; } = [];

    }
}
