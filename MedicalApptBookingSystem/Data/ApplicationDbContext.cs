using MedicalApptBookingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalApptBookingSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {
                
        }

        public DbSet<User> Users { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<DoctorAvailability> DoctorAvailability { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent cascade delete between Apppointment and User (Patient)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient) //  Appt linked to one Patient
                .WithMany(p => p.Appointments) // Patient can have many appts
                .HasForeignKey(a => a.PatientId)  // Link is based on PatientId column
                .OnDelete(DeleteBehavior.Restrict); // If trying to delete a User who is a Patient, and still has linked appts, the deletion will be blocked.

            // Prevent cascade delete between TimeSlot and User (Doctor)
            modelBuilder.Entity<TimeSlot>()
                .HasOne(t => t.Doctor)  // TS linked to one Doctor
                .WithMany(d => d.TimeSlots) // Doctor can have many TS
                .HasForeignKey(t => t.DoctorId) // Linked by DoctorId
                .OnDelete(DeleteBehavior.Restrict); // If deleting Doctor if active TS, the deletion gets prevented.
        }
    }
}
