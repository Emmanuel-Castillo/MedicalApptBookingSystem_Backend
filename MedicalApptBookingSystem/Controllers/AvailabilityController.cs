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

        public AvailabilityController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Endpoint is accessible to Admins and Doctors ONLY
        // Given the request, set the doctor's availability and generate time slots 
        // And for each day of the week provided in the request, create a new Availability row in the db
        // And between StartTime and EndTime, generate 1-hr time slots for patients to book appointments with
        [HttpPost]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> SetDoctorAvailability([FromBody] SetDoctorAvailRequest request)
        {
            try { 
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userId == null || userRole == null) return Forbid("Unauthorized access to this endpoint is prohibited!");
                if (userRole == "Doctor" && int.Parse(userId) != request.DoctorId) return Forbid("Doctor cannot set another doctor's availability.");

                // Verify that request doesn't overlap w/ existing availability
                foreach (var day in request.DaysOfWeek)
                {
                    var overlapAvail = await _context.DoctorAvailability
                        .Where(a => a.DoctorId == request.DoctorId
                            && a.DayOfWeek == day
                            // date range overlap
                            && a.StartDate <= request.EndDate
                            && a.EndDate >= request.StartDate
                            // time range overlap
                            && a.StartTime < request.EndTime
                            && a.EndTime > request.StartTime)
                        .FirstOrDefaultAsync();

                    if (overlapAvail != null)
                    {
                        return BadRequest($"Overlap detected for {day} between " +
                            $"{overlapAvail.StartTime} - {overlapAvail.EndTime} " +
                            $"within {overlapAvail.StartDate:d} to {overlapAvail.EndDate:d}.");
                    }
                }

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

                    // Generate time slots once availability has been saved to db, from the availability StartDate to EndDate
                    var slots = GenerateSlots(docAvail);
                    _context.TimeSlots.AddRange(slots);
                    await _context.SaveChangesAsync();
                }
                return Ok("Availability set and time slots generated.");}
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        private IEnumerable<TimeSlot> GenerateSlots(DoctorAvailability availability)  
        {
            var slots = new List<TimeSlot>();

            // Start from the StartDate
            var currentDate = availability.StartDate;

            // For every day until the untilDate
            while (currentDate < availability.EndDate)
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
