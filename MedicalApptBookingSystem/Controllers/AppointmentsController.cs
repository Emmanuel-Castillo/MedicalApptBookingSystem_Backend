using Azure.Core;
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
    [Route("appointments")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private ConvertToDto _convertToDto;
        public AppointmentsController(ApplicationDbContext context, ConvertToDto convertToDto)
        {
            _context = context;
            _convertToDto = convertToDto;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAppointmentsAsync(int pageNumber = 1, int pageSize = 15)
        {
            try {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userId == null || userRole == null) return Unauthorized("Not authorized to use this endpoint.");

                var query = _context.Appointments
                    .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                    .Include(a => a.TimeSlot)
                    .ThenInclude(ts => ts.Doctor)
                    .ThenInclude(d => d.User);
                var totalCount = await query.CountAsync();

                var appointments = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                var listAppointmentDto = _convertToDto.ConvertToListAppointmentDto(appointments);

                return Ok(new
                {
                    listAppointmentDto,
                    totalCount
                });
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // This endpoint is accessible for Patients and Admins ONLY 
        // Books a patient's appointment
        [HttpPost]
        [Authorize(Roles = "Patient, Admin")]
        public async Task<IActionResult> BookAppointmentAsync([FromBody] BookAppointmentsRequest request)
        {
            try { 
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userId == null) return Unauthorized("Not authorized to use this endpoint.");

                int patientId;

                // If patient id provided in request, check if curr auth User is Admin
                // Assign patientId query param with requested id
                if (!string.IsNullOrEmpty(request.PatientId)) {
                    if (userRole != "Admin")
                    {
                        return Forbid("Attempting to book a Patient's appointment, but the current auth User is not an Admin.");
                    }
                    patientId = int.Parse(request.PatientId);
                }
                // Else assign patientId with curr User id ONLY if User is Patient
                else if (userRole == "Patient")
                {
                    patientId = int.Parse(userId);
                }
                else
                {
                    return BadRequest("Something went wrong with this request. Try again");
                }

                // Check if requested time slot is doesn't exist or if it's been booked
                var timeSlot = await _context.TimeSlots.FindAsync(request.TimeSlotId);
                if (timeSlot == null || timeSlot.IsBooked) return BadRequest("Invalid or already booked time slot.");

                // Set new appointment
                var appointment = new Appointment
                {
                    TimeSlotId = timeSlot.Id,
                    PatientId = patientId,
                    DoctorId = timeSlot.DoctorId
                };

                // Set requested time slot as booked
                timeSlot.IsBooked = true;

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                return Ok("Appointment booked successfully.");
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);

            }
            
        }

        // This endpoint is accessible for Patients and Admins ONLY 
        // Retrieves a patient's appointment
        [HttpGet("{id}")]
        [Authorize(Roles = "Patient, Admin")]
        public async Task<IActionResult> GetAppointmentAsync(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userId == null) return Unauthorized("Not authorized to use this endpoint.");

                // Fetch appointment
                var appt = await _context.Appointments
                    .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                    .Include(a => a.TimeSlot)
                    .ThenInclude(ts => ts.Doctor)
                    .ThenInclude(d => d.User)
                    .FirstOrDefaultAsync(appt => appt.Id == id);

                if (appt == null) return NotFound("Appointment not found.");

                // If curr auth User is Patient, check that heir id matches appt's patient id
                if (userRole == "Patient" && int.Parse(userId) != appt.PatientId)
                {
                    return Forbid("Trying to access another patient's appointment.");
                }

                var apptDto = _convertToDto.ConvertToAppointmentDto(appt);
                return Ok(apptDto);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // This endpoint is accessible for Patients and Admins ONLY 
        // Deletes a patient's appointment
        [HttpDelete("{id}")]
        [Authorize(Roles = "Patient, Admin")]
        public async Task<IActionResult> CancelAppointmentAsync(int id)
        {
            try {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized("Not authorized to use this endpoint.");

                // Find appointment (include TimeSlot to update IsBooked)
                var appointment = await _context.Appointments
                    .Include(a => a.TimeSlot)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointment == null)
                    return NotFound("Appointment not found or not owned by the current user.");

                // Set time slot from appointment property IsBooked back to false
                appointment.TimeSlot.IsBooked = false;

                // Delete appointment
                _context.Appointments.Remove(appointment);

                await _context.SaveChangesAsync();

                return Ok("Appointment successfully cancelled.");
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        // This endpoint is accessible for Doctors and Admins ONLY
        // Adds a note to the request appointment given appt id
        [HttpPost("{id}/notes")]
        [Authorize(Roles = "Doctor, Admin")]
        public async Task<IActionResult> AddNotesToAppointment(int id, [FromBody] UpdateApptNotesRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userId == null || userRole == null) return BadRequest("Attempting to access authorized endpoint.");

                // Find appointment in db
                var appointment = await _context.Appointments
                    .Include(a => a.TimeSlot)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointment == null) return NotFound("Appointment not found.");

                // IF current User is Doctor, validate if appt and doctor have matching doctor ids
                if (userRole == "Doctor" && appointment.TimeSlot.DoctorId != int.Parse(userId))
                    return Forbid("You are not authorized to add notes to this appointment.");

                appointment.Notes = request.UpdatedNotes;
                await _context.SaveChangesAsync();

                return Ok("Notes added successfully.");
            } catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
