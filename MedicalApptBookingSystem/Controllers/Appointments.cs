using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicalApptBookingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Appointments : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public Appointments(ApplicationDbContext context)
        {
            _context = context;
        }

        // This endpoint is authorized only for Patients
        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentsDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            // Check if requested time slot is doesn't exist or if it's been booked
            var timeSlot = await _context.TimeSlots.FindAsync(dto.TimeSlotId);
            if (timeSlot == null || timeSlot.IsBooked) return BadRequest("Invalid or already booked time slot.");

            // Set new appointment
            var appointment = new Appointment
            {
                TimeSlotId = timeSlot.Id,
                PatientId = int.Parse(userId)
            };

            // Set requested time slot as booked
            timeSlot.IsBooked = true;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok("Appointment booked successfully.");
        }

        // This endpoint is authorized only for Patients
        [HttpGet]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetPatientAppointments()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            // Fetch all appointments booked by current autheticated user
            var appointments = await _context.Appointments
                .Where(a => a.PatientId == int.Parse(userId))
                .Include(a => a.TimeSlot)
                .ThenInclude(t => t.Doctor)
                .ToListAsync();

            var appointmentsDto = ConvertToDto(appointments);

            return Ok(appointmentsDto);
        }

        [HttpDelete]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> CancelAppointment(DeleteAppointmentDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();

            var userId = int.Parse(userIdStr);
            // Find appointment (include TimeSlot to update IsBooked)
            var appointment = await _context.Appointments
                .Include(a => a.TimeSlot)
                .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId && a.PatientId == userId);

            if (appointment == null)
                return NotFound("Appointment not found or not owned by the current user");

            // Set time slot from appointment property IsBooked back to false
            appointment.TimeSlot.IsBooked = false;

            // Delete appointment
            _context.Appointments.Remove(appointment);

            await _context.SaveChangesAsync();

            return Ok("Appointment successfully cancelled.");

        }
    
        private List<AppointmentDto> ConvertToDto(List<Appointment> appointments)
        {
            var appointmentsDto = appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                Patient = new UserDto
                {
                    Id = a.Patient.Id,
                    Email = a.Patient.Email,
                    FullName = a.Patient.FullName,
                },
                TimeSlotId = a.TimeSlot.Id,
                TimeSlot = new TimeSlotDto
                {
                    Id = a.TimeSlot.Id,
                    StartTime = a.TimeSlot.StartTime,
                    EndTime = a.TimeSlot.EndTime,
                    Doctor = new UserDto
                    {
                        Id = a.TimeSlot.Doctor.Id,
                        FullName = a.TimeSlot.Doctor.FullName,
                        Email = a.TimeSlot.Doctor.Email
                    },
                    IsBooked = a.TimeSlot.IsBooked,
                },
                Notes = a.Notes
            }).ToList();

            return appointmentsDto;
        }
    }
}
