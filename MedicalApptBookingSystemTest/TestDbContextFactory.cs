using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalApptBookingSystemTest
{
    public static class TestDbContextFactory
    {
        // Create in-memory database and seed it with data for each test case
        public static ApplicationDbContext Create()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            Seed(context);

            return context;
        }

        private static void Seed(ApplicationDbContext context)
        {

            // SEEDING USERS
            context.Users.AddRange(
                new User
                {
                    FullName = "Patient1",
                    Email = "patient1@example.com",
                    Id = 1,
                    PasswordHash = Guid.NewGuid().ToString(),
                    Role = UserRole.Patient
                },
                new User
                {
                    FullName = "Doctor1",
                    Email = "doctor1@example.com",
                    Id = 2,
                    PasswordHash = Guid.NewGuid().ToString(),
                    Role = UserRole.Doctor
                },
                new User
                {
                    FullName = "Patient2",
                    Email = "patient2@example.com",
                    Id = 3,
                    PasswordHash = Guid.NewGuid().ToString(),
                    Role = UserRole.Patient
                },
                new User
                {
                    FullName = "Doctor2",
                    Email = "doctor2@example.com",
                    Id = 4,
                    PasswordHash = Guid.NewGuid().ToString(),
                    Role = UserRole.Doctor
                }
            );

            // SEEDING TIME SLOTS
            context.TimeSlots.AddRange(
                new TimeSlot
                {
                    Id = 1,
                    Date = DateOnly.FromDateTime(DateTime.Today),
                    StartTime = TimeOnly.FromDateTime(DateTime.Now),
                    EndTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(1)),
                    DoctorId = 2,
                    IsBooked = true,
                },
                new TimeSlot
                {
                    Id = 2,
                    Date = DateOnly.FromDateTime(DateTime.Today),
                    StartTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(1)),
                    EndTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(2)),
                    DoctorId = 2,
                    IsBooked = true,
                },
                new TimeSlot
                {
                    Id = 3,
                    Date = DateOnly.FromDateTime(DateTime.Today),
                    StartTime = TimeOnly.FromDateTime(DateTime.Now),
                    EndTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(1)),
                    DoctorId = 4,
                    IsBooked = false,
                },
                new TimeSlot
                {
                    Id = 4,
                    Date = DateOnly.FromDateTime(DateTime.Today),
                    StartTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(1)),
                    EndTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(2)),
                    DoctorId = 4,
                    IsBooked = false,
                }
                
                );

            // SEEDING APPOINTMENTS
            context.Appointments.AddRange(
                new Appointment
                {
                    Id = 1,
                    PatientId = 1,
                    DoctorId = 2,
                    TimeSlotId = 1,
                    Notes = null
                },
                new Appointment
                {
                    Id = 2,
                    PatientId = 3,
                    DoctorId = 2,
                    TimeSlotId = 2,
                    Notes = null
                }
                );

            // Save additions to fake db
            context.SaveChanges();
        }
    }
}
