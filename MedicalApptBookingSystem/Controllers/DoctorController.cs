using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO.Reponses;
using MedicalApptBookingSystem.Models;
using MedicalApptBookingSystem.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicalApptBookingSystem.Controllers
{
    [Route("doctors")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        ApplicationDbContext _context;
        ConvertToDto _convertToDto;
        public DoctorController(ApplicationDbContext context, ConvertToDto convertToDto)
        {
            _context = context;
            _convertToDto = convertToDto;
        }

        // Endpoint accessible for Admins ONLY
        // Retrieve ALL Doctors
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllDoctorsAsync()
        {
            try
            {
                var users = await _context.Users.Where(u => u.Role == UserRole.Doctor).ToListAsync();
                var usersDto = _convertToDto.ConvertToListUserDto(users);
                return Ok(usersDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoint accessible for Admins and Doctors ONLY
        // Retrieves specific Doctor information for their home page
        // Returns their User information, and a list of time slots for the next seven days
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> GetDoctorAsync(int id)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId == null || currentUserRole == null)
                    return BadRequest("User Id Not Defined!");

                if (currentUserRole == "Doctor" && int.Parse(currentUserId) != id)
                    return Forbid("Trying to access another doctor's information.");

                // Fetch doctor from db
                var doctor = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (doctor == null) return NotFound("User not found!");

                // Fetch all time slots
                // Whose StartTime is between now and the next seven days
                // Order them by earliest date
                var timeSlots = await _context.TimeSlots
                    .Where(ts => ts.DoctorId == id)
                    .Where(ts => ts.StartTime >= DateTime.Now && ts.StartTime <= DateTime.Now.AddDays(7))
                    .OrderByDescending(ts => ts.StartTime)
                    .ToListAsync();

                var dto = new GetDoctorInfoResponse
                {
                    Doctor = _convertToDto.ConvertToUserDto(doctor),
                    UpcomingTimeSlots = _convertToDto.ConvertToListTimeSlotDto(timeSlots)
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoint accessible for Admins and Doctors ONLY
        // Retrieves list of specific Doctor's time slots and totalCount of all time slots
        // Pagination is implemented, default settings make this endpoint return a page w/ a max of 10 time slots
        [HttpGet("{id}/timeslots")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> GetDoctorTimeSlotsAsync(int id, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // Fetch current auth User (Doctor) Id
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userId == null || userRole == null) return Unauthorized("Not authorized to use this endpoint.");
                if (userRole == "Doctor" && int.Parse(userId) != id) return Forbid("Attempting to access another doctor's time slots.");

                // Query to fetch all time slots by this doctor
                var query = _context.TimeSlots
                    .Where(ts => ts.DoctorId == id)
                    .Include(ts => ts.Doctor)
                    .OrderBy(t => t.StartTime);

                // Total count calculates how many time slots exist
                var totalCount = await query.CountAsync();

                // Grabs #(pageSize) of timeSlots starting at page #(pageNumber)
                var slots = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var timeSlotDtos = _convertToDto.ConvertToListTimeSlotDto(slots);

                return Ok(new
                {
                    timeSlotDtos,
                    totalCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoint accessible for Admins and Doctors ONLY
        // Retrieves list of specified Doctor's availability and total count of avail rows
        // Pagination is implemented, default settings make this endpoint return a page w/ a max of 10 time slots
        [HttpGet("{id}/availability")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> GetDoctorAvailabilityAsync(int id, int pageNumber = 1, int pageSize = 10) {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userId == null || userRole == null) return Forbid("Unauthorized access to this endpoint is prohibited!");
                if (userRole == "Doctor" && int.Parse(userId) != id) return Forbid("Doctor cannot access another doctor's availability.");

                // Query to fetch all availabilities set by this doctor
                var query = _context.DoctorAvailability
                    .Where(d => d.DoctorId == id);

                // Total count calculates how avail rows exist
                var totalCount = await query.CountAsync();

                // Grabs #(pageSize) of availabilities starting at page #(pageNumber)
                var availabilities = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var availabilitesDto = _convertToDto.ConvertToListDoctorAvailDto(availabilities);
                return Ok(new {availabilitesDto, totalCount});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoint accessible for Admins and Doctors ONLY
        // Retrieves specific Doctor's availability for the current week
        [HttpGet("{id}/availability/thisweek")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> GetDoctorAvailabilityThisWeekAsync(int id) {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userId == null || userRole == null) return Forbid("Unauthorized access to this endpoint is prohibited!");
                if (userRole == "Doctor" && int.Parse(userId) != id) return Forbid("Doctor cannot access another doctor's availability.");

                var today = DateTime.Today;
                int delta = DayOfWeek.Monday - today.DayOfWeek;
                var weekStart = today.AddDays(delta);
                var weekEnd = weekStart.AddDays(7).AddSeconds(-1);
                var availabilities = await _context.DoctorAvailability
                    .Where(d => d.DoctorId == id &&
                        d.StartDate <= weekEnd &&
                        d.EndDate >= weekStart)
                    .ToListAsync();

                var availabilitesDto = _convertToDto.ConvertToListDoctorAvailDto(availabilities);

                return Ok(availabilitesDto);
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }

        }
    }
}
