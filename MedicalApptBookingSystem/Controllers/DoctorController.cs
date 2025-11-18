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
        // Retrieve ALL doctor profiles
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllDoctorsAsync()
        {
            try
            {
                var doctors = await _context.Doctors.Include(dp => dp.User).ToListAsync();
                var listDoctorDtos = _convertToDto.ConvertToListDoctorDto(doctors);
                return Ok(listDoctorDtos);

                //var users = await _context.Users.Where(u => u.Role == UserRole.Doctor).ToListAsync();
                //var usersDto = _convertToDto.ConvertToListUserDto(users);
                //return Ok(usersDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoint accessible for Admins and Doctors ONLY
        // Retrieves doctor's profile and booked time slots for the next two weeks
        [HttpGet("{doctorUserId}")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> GetDoctorAsync(int doctorUserId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (currentUserId == null || currentUserRole == null)
                    return BadRequest("User Id or Role Not Defined!");

                if (currentUserRole == "Doctor" && int.Parse(currentUserId) != doctorUserId)
                    return Forbid("Trying to access another Doctor's information.");

                // Fetch doctor's profile from db
                var doctor = await _context.Doctors.Include(dp => dp.User).Include(d => d.TimeSlots).FirstOrDefaultAsync(dp => dp.UserId == doctorUserId);
                if (doctor == null) return NotFound("User not found!");

                // End date limit for timeSlots acquired
                var today = DateOnly.FromDateTime(DateTime.Today);
                var twoWeeksFromToday = today.AddDays(14);


                // Fetch all time slots from now through the next two weeks
                // Order them by earliest date
                //var timeSlots = doctor.TimeSlots.ToList();
                var timeSlots = doctor.TimeSlots
                    .Where(ts => ts.IsBooked &&
                                 ts.Date >= today &&
                                 ts.Date <= twoWeeksFromToday)
                    .OrderBy(ts => ts.Date)
                    .ThenBy(ts => ts.StartTime)
                    .ToList();

                var dto = new GetDoctorInfoResponse
                {
                    DoctorProfile = _convertToDto.ConvertToDoctorDto(doctor),
                    BookedTimeSlotsNextTwoWeeks = _convertToDto.ConvertToListTimeSlotDto(timeSlots)
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
        [HttpGet("{doctorId}/timeslots")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> GetDoctorTimeSlotsAsync(int doctorId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // Fetch current auth User (Doctor) Id
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userId == null || userRole == null) return Unauthorized("Not authorized to use this endpoint.");

                var doctor = await _context.Doctors.Include(d => d.TimeSlots).Include(d => d.User).FirstOrDefaultAsync(d => d.Id == doctorId);
                if (doctor == null) return NotFound("Doctor not found!");
                if (userRole == "Doctor" && doctor.UserId != int.Parse(userId)) return Forbid("Attempting to access another doctor's time slots.");

                // Total count calculates how many time slots exist
                var totalCount = doctor.TimeSlots.Count;

                // Grabs #(pageSize) of timeSlots starting at page #(pageNumber)
                var slots = doctor.TimeSlots
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize).ToList();

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
                if (userId == null || userRole == null) return Unauthorized("Not authorized to use this endpoint.");

                var doctor = await _context.Doctors.Where(d => d.UserId == int.Parse(userId)).FirstOrDefaultAsync();
                if (doctor == null) return NotFound("Doctor not found!");
                if (userRole == "Doctor" && doctor.Id != id) return Forbid("Doctor cannot access another doctor's availability.");

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
        [HttpGet("{doctorId}/availability/thisweek")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> GetDoctorAvailabilityThisWeekAsync(int doctorId) {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userId == null || userRole == null) return Unauthorized("Not authorized to use this endpoint.");

                var doctor = await _context.Doctors.Where(d => d.Id == doctorId).FirstOrDefaultAsync();
                if (doctor == null) return NotFound("Doctor not found!");
                if (userRole == "Doctor" && doctor.UserId != int.Parse(userId)) return Forbid("Doctor cannot access another doctor's availability.");

                var today = DateOnly.FromDateTime(DateTime.Today);
                int delta = DayOfWeek.Monday - today.DayOfWeek;
                var weekStart = today.AddDays(delta);
                var weekEnd = weekStart.AddDays(7);
                var availabilities = await _context.DoctorAvailability
                    .Where(d => d.DoctorId == doctorId &&
                        d.StartDate <= weekEnd &&
                        d.EndDate >= weekStart)
                    .Include(d => d.Doctor)
                    .ThenInclude(d => d.User)
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
