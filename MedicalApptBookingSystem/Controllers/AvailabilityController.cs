using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO.Requests;
using MedicalApptBookingSystem.Models;
using MedicalApptBookingSystem.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicalApptBookingSystem.Controllers
{
    [Route("availability")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly ConvertToDto _convertToDto;

        public AvailabilityController(ApplicationDbContext context, ConvertToDto convertToDo)
        {
            _context = context;
            _convertToDto = convertToDo;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllDoctorsAvailabilities()
        {
            var availabilites = await _context.DoctorAvailability.ToListAsync();

            var availabilitesDto = _convertToDto.ConvertToListDoctorAvailDto(availabilites);

            return Ok(availabilitesDto);
        }

        [HttpGet("current/{id}")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> GetADoctorsCurrentAvailability(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId == null || userRole == null) return Forbid("Unauthorized access to this endpoint is prohibited!");

            if (userRole == "Doctor" && int.Parse(userId) != id) return Forbid("Doctor cannot access another doctor's availability.");

            var availabilites = await _context.DoctorAvailability.Where(a => a.DoctorId == id).Include(a => a.Doctor).ToListAsync();

            var availabilitesDto = _convertToDto.ConvertToListDoctorAvailDto(availabilites);

            return Ok(availabilitesDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> SetDoctorAvailability([FromBody] SetDoctorAvailRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId == null || userRole == null) return Forbid("Unauthorized access to this endpoint is prohibited!");

            if (userRole == "Doctor" && int.Parse(userId) != request.DoctorId) return Forbid("Doctor cannot set another doctor's availability.");

            // For each day in the request list
            foreach (var day in request.DaysOfWeek)
            {
                // Set avail for doctor
                var docAvail = new DoctorAvailability
                {
                    DoctorId = request.DoctorId,
                    DayOfWeek = day,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                };

                _context.DoctorAvailability.Add(docAvail);
                await _context.SaveChangesAsync();

                // Generate time slots once availability has been saved to db, up to one month in advance.
                var slots = GenerateSlots(docAvail, DateTime.UtcNow.AddMonths(1));
                _context.TimeSlots.AddRange(slots);
                await _context.SaveChangesAsync();
            }

            return Ok("Availability set and time slots generated.");

        }

        private IEnumerable<TimeSlot> GenerateSlots(DoctorAvailability availability, DateTime untilDate)  
        {
            var slots = new List<TimeSlot>();

            // Start from the StartDate
            var currentDate = availability.StartDate;

            // For every day until the untilDate
            while (currentDate < untilDate)
            {
                // Create time slots ONLY for DayOfWeek specified in the availability obj
                if (currentDate.DayOfWeek == availability.DayOfWeek)
                {
                    var currSlotTime = availability.StartTime;
                    // And for every hour starting from StartTime to EndTime
                    while (currSlotTime < availability.EndTime)
                    {
                        // Create a new time slot that lasts an hour, and make it available to book
                        slots.Add(new TimeSlot
                        {
                            DoctorId = availability.DoctorId,
                            StartTime = currentDate.Add(currSlotTime),
                            EndTime = currentDate.Add(currSlotTime).Add(TimeSpan.FromHours(1)),
                            IsBooked = false,
                        });

                        // Shift currSlotTime forward by an hour to create next available time slot
                        currSlotTime = currSlotTime.Add(TimeSpan.FromHours(1));
                    }
                }
                // Shift currentDate forward by a day
                currentDate = currentDate.AddDays(1);
            }

            return slots;
        }
    }
}
