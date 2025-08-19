using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.DTO.Reponses;
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
    [Route("users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        ApplicationDbContext _context;
        ConvertToDto _convertToDto;
        public UsersController(ApplicationDbContext context, ConvertToDto convertToDto)
        {
            _context = context;
            _convertToDto = convertToDto;
        }

        // Endpoint accessible to all authorized Users
        // Retrieve UserDto given user id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserAsync(int id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null) return NotFound("User not found!");

                var userDto = _convertToDto.ConvertToUserDto(user);
                return Ok(userDto);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoint accessible for Admins ONLY
        // Edit User's data
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUserAsync(ChangeUserRequest request)
        {
            try {
                // Find user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(request.Id));

                if (user == null) return NotFound("User not found!");

                // Check if request NewEmail has been taken by another User!
                var userWithNewEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.NewEmail);

                if (userWithNewEmail != null) return BadRequest("This requested new email has already been taken!");

                user.FullName = request.NewFullName;
                user.Email = request.NewEmail;

                await _context.SaveChangesAsync();

                return Ok("User has been updated!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoint authorized for Admins ONLY
        // Retrieve ALL users
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                var usersDto = _convertToDto.ConvertToListUserDto(users);
                return Ok(usersDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        // Endpoint authorized for Admins and Doctors ONLY
        // Retrieve ONLY ALL patients
        [HttpGet("patients")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> GetAllPatientsAsync()
        {
            try
            {
                var users = await _context.Users.Where(u => u.Role == UserRole.Patient).ToListAsync();
                var usersDto = _convertToDto.ConvertToListUserDto(users);
                return Ok(usersDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoint accessible by any authorized User
        // Retrieve ONLY ALL doctors
        [HttpGet("doctors")]
        [Authorize]
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

        // Endpoint accessible by any authorized User
        // Retrieve a patient's information (User data & appointments booked)
        [HttpGet("patients/{patientId}")]
        [Authorize(Roles = "Patient, Admin")]
        public async Task<IActionResult> GetPatientInfoAsync(string patientId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId == null)
                {
                    return BadRequest("User Id Not Defined!");
                }

                if (currentUserRole == "Patient" && currentUserId != patientId)
                {
                    return Forbid("Trying to access another patient's information.");
                }

                int patientIdInt = int.Parse(patientId);

                // Finally, fetch patient from db
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == patientIdInt);

                if (user == null) {
                    return NotFound("User not found!");
                }

                // Fetch appts booked by patient
                var appointments = await _context.Appointments
                .Where(a => a.PatientId == patientIdInt)
                .Include(a => a.TimeSlot)
                .ThenInclude(t => t.Doctor)
                .ToListAsync();

                var dto = new GetPatientInfoResponse
                {
                    Patient = _convertToDto.ConvertToUserDto(user),
                    Appointments = _convertToDto.ConvertToListAppointmentDto(appointments)
                };

                return Ok(dto);
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }
        }

        // Endpoint accessible any authorized User
        // Retrieve a doctor's information (User data & time slots created)
        [HttpGet("doctors/{doctorId}")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> GetDoctorInfoAsync(string doctorId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId == null)
                {
                    return BadRequest("User Id Not Defined!");
                }

                if (currentUserRole == "Doctor" && currentUserId != doctorId)
                {
                    return Forbid("Trying to access another doctor's information.");
                }

                int doctorIdInt = int.Parse(doctorId);

                // Fetch doctor from db
                var doctor = await _context.Users.FirstOrDefaultAsync(u => u.Id == doctorIdInt);

                if (doctor == null)
                {
                    return NotFound("User not found!");
                }

                // Fetch all time slots for the next seven days
                var timeSlots = await _context.TimeSlots
                    .Where(ts => ts.DoctorId == doctorIdInt)
                    .Where(ts => ts.StartTime >= DateTime.Now && ts.StartTime <= DateTime.Now.AddDays(7))
                    .OrderByDescending(ts => ts.StartTime)
                    .ToListAsync();

                var dto = new GetDoctorInfoResponse
                {
                    Doctor = _convertToDto.ConvertToUserDto(doctor),
                    TimeSlots = _convertToDto.ConvertToListTimeSlotDto(timeSlots)
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
