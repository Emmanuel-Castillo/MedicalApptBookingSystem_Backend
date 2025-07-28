using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicalApptBookingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeSlotsController : ControllerBase
        // ControllerBase class has a built in property: 
        // public ClaimsPrincipal User { get; set }
        // This represents the current authenticated user
        // ClaimsPrincipal is a .NET class that contains the 
        // key-value pairs (claims) extracted from a JWT token 
        // that the client sends with the HTTP request.
    {
        private readonly ApplicationDbContext _context;

        public TimeSlotsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // This api endpoint is only authorized for Doctors and Admins
        [HttpGet("doctor")]
        [Authorize(Roles = "Doctor, Admin")]
        public async Task<IActionResult> GetOwnTimeSlots()
        {
            // Check if Doctor Id claim exists in Jwt token claim
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            // Then check for User Role claim and see if it's an authorized Doctor or Admin (not a Patient)
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == null || userRole != UserRole.Patient.ToString()) return Unauthorized();

            var doctorId = int.Parse(userId);
            // Fetch all time slots by this doctor
            var timeSlots = await _context.TimeSlots
                .Include(ts => ts.Doctor)
                .Where(ts => ts.DoctorId == doctorId)
                .ToListAsync();

            var timeSlotDtos = convertToDto(timeSlots);

            return Ok(timeSlotDtos);
        }

        // This api endpoint is only authorized for Doctors and Admins
        [HttpPost("create")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> CreateTimeSlot([FromBody] CreateTimeSlotRequest request)
        {
            // [FromBody] tells .NET Core to deserialize the incoming HTTP request body into a DTO
            // User is the current authenticated user
            // What this line is doing is, using the authenticated token, which has been verified
            // through the middleware, the claim "NameIdentifier" gets extracted which contains the user's Id
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();

            int doctorId = int.Parse(userIdStr);
            // Check for conflicts with existing slots for this particular Doctor
            bool conflict = await _context.TimeSlots.AnyAsync(slot =>
                slot.DoctorId == doctorId &&
                slot.StartTime < request.EndTime &&
                request.StartTime < slot.EndTime
            );

            if (conflict) return BadRequest("Time slot overlaps witha an existing slot.");

            // Create new TimeSlot
            var timeSlot = new TimeSlot
            {
                DoctorId = doctorId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsBooked = false
            };

            // Save new timeslot to db
            _context.TimeSlots.Add(timeSlot);
            await _context.SaveChangesAsync();

            return Ok(timeSlot);
        }
        
        // This api endpoint is only authorized for Doctors and Admins
        [HttpDelete]
        [Authorize(Roles = "Doctor, Admin")]
        public async Task<IActionResult> DeleteTimeSlot(DeleteTimeSlotDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();

            var doctorId = int.Parse(userIdStr);

            // Check if appointment has been booked in that time slot
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync (a => a.TimeSlotId == dto.TimeSlotId);

            if (appointment != null) return BadRequest("Cannot delete a time slot with a booked appointment.");

            // If appointment isn't booked for this time slot, fetch the time slot itself
            var timeSlot = await _context.TimeSlots
                .FirstOrDefaultAsync(a => a.Id == dto.TimeSlotId);

            if (timeSlot == null) return NotFound("Time slot not found.");

            // Delete time slot from db if it exists
            _context.TimeSlots.Remove(timeSlot);
            await _context.SaveChangesAsync();

            return Ok("Time slot deleted successfully");
        
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableTimeSlots()
        {
            var availableTimeSlots = await _context.TimeSlots
                .Include(t => t.Doctor)
                .Where(t => !t.IsBooked)
                .ToListAsync();

            var availableTimeSlotDtos = convertToDto(availableTimeSlots);
            return Ok(availableTimeSlotDtos);
        }

        
        private List<TimeSlotDto> convertToDto(List<TimeSlot> timeSlots)
        {
            var timeSlotDtos = timeSlots.Select(t => new TimeSlotDto
            {
                Id = t.Id,
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                IsBooked = t.IsBooked,
                Doctor = new UserDto
                {
                    Id = t.Doctor.Id,
                    FullName = t.Doctor.FullName,
                    Email = t.Doctor.Email
                }

            }).ToList();

            return timeSlotDtos;
        }
    }
}
