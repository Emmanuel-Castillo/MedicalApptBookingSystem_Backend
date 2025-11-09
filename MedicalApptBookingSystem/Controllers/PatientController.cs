using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.DTO.Reponses;
using MedicalApptBookingSystem.DTO.Requests;
using MedicalApptBookingSystem.Models;
using MedicalApptBookingSystem.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicalApptBookingSystem.Controllers
{
    [Route("patients")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        ApplicationDbContext _context;
        ConvertToDto _convertToDto;

        public PatientController(ApplicationDbContext context, ConvertToDto convertToDto)
        {
            _context = context;
            _convertToDto = convertToDto;
        }

        // Endpoint authorized for Admins ONLY
        // Retrieve ONLY ALL patients profiles
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPatientsAsync()
        {
            try
            {
                var patients = await _context.Patients.Include(pp => pp.User).ToListAsync();
                var usersDto = _convertToDto.ConvertToListPatientDto(patients);
                return Ok(usersDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoint accessible for Admins and Patients ONLY
        // Retrieve a Patient's information (User data & appointments booked for the week)
        [HttpGet("{userId}")]
        [Authorize(Roles = "Admin, Patient")]
        public async Task<IActionResult> GetPatientByUserIdAsync(int userId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId == null || currentUserRole == null)
                    return BadRequest("User Id Not Defined!");

                if (currentUserRole == "Patient" && int.Parse(currentUserId) != userId)
                    return Forbid("Trying to access another patient's information.");

                // Finally, fetch patient from db
                var patient = await _context.Patients.Include(pp => pp.User).FirstOrDefaultAsync(pp => pp.UserId == userId);
                if (patient == null) return NotFound("User not found!");

                // Fetch appts booked by patient for the current week
                // Ordered by StartTime of the appt
                var today = DateOnly.FromDateTime(DateTime.Today);
                int delta = DayOfWeek.Monday - today.DayOfWeek;
                var weekStart = today.AddDays(delta);
                var weekEnd = weekStart.AddDays(7);

                var appointments = await _context.Appointments
                .Where(a => a.PatientId == userId &&
                        a.TimeSlot.Date <= weekEnd &&
                        a.TimeSlot.Date >= weekStart)
                .Include(a => a.TimeSlot)
                .ThenInclude(t => t.Doctor)
                .OrderBy(a => a.TimeSlot.StartTime)
                .ToListAsync();

                var dto = new GetPatientInfoResponse
                {
                    PatientProfile = _convertToDto.ConvertToPatientDto(patient),
                    AppointmentsThisWeek = _convertToDto.ConvertToListAppointmentDto(appointments)
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoint accessible for Admins and Patients ONLY
        // Retrieve all of a specified Patient's appointments
        // Pagination included. By default, a page will return 10 appt
        [HttpGet("{patientId}/appointments")]
        [Authorize(Roles = "Admin, Patient")]
        public async Task<IActionResult> GetPatientAppointmentsAsync(int patientId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // Fetch current auth User (Patient or Admin) Id
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userId == null || userRole == null) return Unauthorized("Not authorized to use this endpoint.");

                var patient = await _context.Patients.Where(p => p.Id == patientId).FirstOrDefaultAsync();
                if (patient == null) return NotFound("Patient not found!");
                if (userRole == "Patient" && int.Parse(userId) != patient.UserId) return Forbid("Attempting to access another patient's appointments.");

                // Query to fetch all appointments for patient
                var query = _context.Appointments
                    .Where(a => a.PatientId == patientId)
                    .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                    .Include(a => a.TimeSlot)
                    .ThenInclude(ts => ts.Doctor)
                    .ThenInclude(d => d.User)
                    .OrderBy(a => a.TimeSlot.StartTime);

                // Total count calculates how many appointments patient has booked
                var totalCount = await query.CountAsync();

                // Grabs #(pageSize) of appointments starting at page #(pageNumber)
                var appointments = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var appointmentDtos = _convertToDto.ConvertToListAppointmentDto(appointments);

                // Setup response obj
                var response = new GetPatientAppointmentsResponse
                {
                    Appointments = appointmentDtos,
                    TotalCount = totalCount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoint accessible for Admins and Patients ONLY
        // Update a specified Patient's information
        [HttpPost("{patientId}")]
        [Authorize(Roles = "Admin, Patient")]
        public async Task<IActionResult> UpdatePatientInformation(int patientId, UpdatePatientInformationRequest request)
        {
            try
            {
                // Fetch current auth User (Patient or Admin) Id
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userId == null || userRole == null) return Unauthorized();

                var patient = await _context.Patients.Where(p => p.Id == patientId).FirstOrDefaultAsync();
                if (patient == null) return NotFound("Patient not found!");
                if (userRole == "Patient" && int.Parse(userId) != patient.UserId) return Forbid("Attempting to update another patient's information.");

                patient.HeightImperial = request.newHeightImperial;
                patient.WeightImperial = request.newWeightImperial;

                await _context.SaveChangesAsync();
                var patientDto = _convertToDto.ConvertToPatientDto(patient);

                return Ok(patientDto);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
