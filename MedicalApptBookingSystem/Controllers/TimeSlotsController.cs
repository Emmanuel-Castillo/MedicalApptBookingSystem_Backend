using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO.Requests;
using MedicalApptBookingSystem.Models;
using MedicalApptBookingSystem.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicalApptBookingSystem.Controllers
{
    [Route("timeslots")]
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
        private ConvertToDto _convertToDto;

        public TimeSlotsController(ApplicationDbContext context, ConvertToDto convertToDto)
        {
            _context = context;
            _convertToDto = convertToDto;
        }

        // Endpoint is authorized for Doctors and Admins ONLY
        // Meant to current authorized User (Role = Doctor) to retrieve own time slots
        // Or if Admin, fetch time slots for particular Doctor
        [HttpGet]
        [Authorize(Roles = "Doctor, Admin")]
        public async Task<IActionResult> GetTimeSlots([FromBody] GetDoctorTimeSlotsRequest request)
        {
            try
            {
                // Fetch current auth User (Doctor) Id
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userId == null) return Unauthorized("Not authorized to use this endpoint.");

                int doctorId;

                // Assign doctorId with requested id if Admin
                if (!string.IsNullOrEmpty(request.DoctorId))
                {
                    if (userRole != "Admin")
                    {
                        return Forbid();
                    }
                    doctorId = int.Parse(request.DoctorId);
                }

                // Else, assign doctorId with curr auth User ONLY if User is Doctor
                else if (userRole == "Doctor")
                {
                    doctorId = int.Parse(userId);
                }

                else
                {
                    return BadRequest("Something went wrong with this request. Try again.");
                }

                    // Fetch all time slots by this doctor
                    var timeSlots = await _context.TimeSlots
                        .Where(ts => ts.DoctorId == doctorId)
                        .ToListAsync();

                var timeSlotDtos = _convertToDto.ConvertToListTimeSlotDto(timeSlots);

                return Ok(timeSlotDtos);
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }
            
        }

        // Endpoint is authorized for Doctors and Admins
        // Allows current authorized User (Role = Doctor) to create their own time slot
        // Or if admin, create a time slot for a given Doctor
        [HttpPost]
        [Authorize(Roles = "Doctor, Admin")]
        public async Task<IActionResult> CreateTimeSlot([FromBody] CreateTimeSlotRequest request)
        {

            try { 
                // [FromBody] tells .NET Core to deserialize the incoming HTTP request body into a DTO
                // User is the current authenticated user
                // What this line is doing is, using the authenticated token, which has been verified
                // through the middleware, the claim "NameIdentifier" gets extracted which contains the user's Id
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userId == null) return Unauthorized("Not authorized to use this endpoint.");

                int doctorId;

                // If doctor id provided, check if curr User is Admin
                // Assign doctorId to requested id
                if (!string.IsNullOrEmpty (request.DoctorId))
                {
                    if (userRole != "Admin")
                    {
                        return Forbid("Trying to create a Doctor's Time Slot, but the current authorized User is not an Admin.");
                    }
                    doctorId = int.Parse(request.DoctorId);
                }

                // Else, assign doctorId to curr User id ONLY if User is a Doctor
                else if (userRole == "Doctor")
                {
                    doctorId = int.Parse(userId);
                }

                else
                {
                    return BadRequest("Something when wrong with the request. Try again.");
                }

                    return await CreateTimeSlot(doctorId, request);
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }
        }

        // Endpoint is only authorized for Doctors and Admins
        // Deletes any particular Time Slot by their Id
        [HttpDelete]
        [Authorize(Roles = "Doctor, Admin")]
        public async Task<IActionResult> DeleteTimeSlot(DeleteTimeSlotRequest dto)
        {
            try { 
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdStr == null) return Unauthorized("Not authorized to use this endpoint.");

                // Check if appointment has been booked in that time slot
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync (a => a.TimeSlotId == dto.TimeSlotId);

                if (appointment != null) return BadRequest("Cannot delete a time slot with a booked appointment.");

                // If appointment isn't booked for this time slot, fetch the time slot itself
                var timeSlot = await _context.TimeSlots
                    .FirstOrDefaultAsync(a => a.Id == dto.TimeSlotId);

                if (timeSlot == null) return NotFound("Time slot not found.");

                // Delete time slot from db
                _context.TimeSlots.Remove(timeSlot);
                await _context.SaveChangesAsync();

                return Ok("Time slot deleted successfully");}
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }
        }

        // Endpoint accessible by any authorized Users
        // Retrives ALL AVAILABLE time slots
        [HttpGet("available")]
        [Authorize]
        public async Task<IActionResult> GetAvailableTimeSlots()
        {
            try {
                var availableTimeSlots = await _context.TimeSlots
                .Include(t => t.Doctor)
                .Where(t => !t.IsBooked)
                .ToListAsync();

                var availableTimeSlotDtos = _convertToDto.ConvertToListTimeSlotDto(availableTimeSlots);
                return Ok(availableTimeSlotDtos); 
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
            
        }

        // Utility function to create a new Time Slot
        // Invoked from doctor creating their own time slot, or an Admin creating one for a doctor
        private async Task<IActionResult> CreateTimeSlot(int doctorId, CreateTimeSlotRequest dto)
        {

            try { // Check for conflicts with existing slots for this particular Doctor
            bool conflict = await _context.TimeSlots.AnyAsync(slot =>
                slot.DoctorId == doctorId &&
                slot.StartTime < dto.EndTime &&
                dto.StartTime < slot.EndTime
            );

            if (conflict) return BadRequest("Time slot overlaps witha an existing slot.");

            // Create new TimeSlot
            var timeSlot = new TimeSlot
            {
                DoctorId = doctorId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsBooked = false
            };

            // Save new timeslot to db
            _context.TimeSlots.Add(timeSlot);
            await _context.SaveChangesAsync();

            return Ok(timeSlot);}
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }

        }
        
    }
}
