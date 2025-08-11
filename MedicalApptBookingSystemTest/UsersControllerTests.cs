using MedicalApptBookingSystem.Controllers;
using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.DTO.Reponses;
using MedicalApptBookingSystem.Models;
using MedicalApptBookingSystem.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MedicalApptBookingSystemTest
{
    public class UsersControllerTests
    {

        private readonly ApplicationDbContext _context;
        private readonly ConvertToDto _convertToDto;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            // Setup test db context
            _context = TestDbContextFactory.Create();

            // Set up the mock AuthService
            _convertToDto = new ConvertToDto();

            // Inject the mock AuthService into controller
            _controller = new UsersController(_context, _convertToDto);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsAllUsers()
        {
            // Act
            var result = await _controller.GetAllUsersAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);       // Returns Ok
            var users = Assert.IsType<List<UserDto>>(okResult.Value);   // OkResult obj contains list of UsersDto
            Assert.Equal(4, users.Count());                             // There are only 4 in the list
        }

        [Fact]
        public async Task GetAllPatients_ReturnsAllPatients()
        {
            // Act
            var result = await _controller.GetAllPatientsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);           // Returns Ok
            var patients = Assert.IsType<List<UserDto>>(okResult.Value);   // OkResult obj contains list of UsersDto
            Assert.Equal(2, patients.Count());                             // There are only 2 in the list
        }

        [Fact]
        public async Task GetAllDoctors_ReturnsAllDoctors()
        {
            // Act
            var result = await _controller.GetAllDoctorsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);           // Returns Ok
            var doctors = Assert.IsType<List<UserDto>>(okResult.Value);   // OkResult obj contains list of UsersDto
            Assert.Equal(2, doctors.Count());                             // There are only 2 in the list
        }

        // GRABBING SPECIFIC PATIENT DETAILS

        [Fact]
        public async Task GetPatientInfo_ReturnsSpecificPatient()
        {
            // Arrange
            var patientId = "1";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, patientId),
                new Claim(ClaimTypes.Role, "Patient")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.GetPatientInfoAsync(patientId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);           // Returns Ok
            var patientInfo = Assert.IsType<GetPatientInfoResponse>(okResult.Value);   // OkResult obj contains patient info
            
            var patient = Assert.IsType<UserDto>(patientInfo.Patient);
            var appointments = Assert.IsType<List<AppointmentDto>>(patientInfo.Appointments);

            // Assert patient info is correct
            Assert.Equal(1, patient.Id);
            Assert.Equal("Patient1", patient.FullName);
            Assert.Equal("patient1@example.com", patient.Email);
            Assert.Equal(UserRole.Patient.ToString(), patient.Role);

            Assert.Single(appointments);
        }

        [Fact]
        public async Task AsUnauthenticatedUser_GetPatientInfo_ReturnsBadRequest()
        {
            // Arrange (do not setup user)
            var patientId = "1";

            // Act
            var result = await _controller.GetPatientInfoAsync(patientId);

            // Assert
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("Object reference not set to an instance of an object.", badRequestRes.Value);

        }

        [Fact]
        public async Task WithoutUserId_GetPatientInfo_ReturnsBadRequest()
        {
            // Arrange
            var patientId = "1";

            // Setting up User without NameIdentifier claim (no user id)
            var claims = new List<Claim> { };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };


            // Act
            var result = await _controller.GetPatientInfoAsync(patientId);

            // Assert
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("User Id Not Defined!", badRequestRes.Value);

        }

        [Fact]
        public async Task AsAnotherPatient_GetPatientInfo_ReturnsForbid()
        {
            // Arrange
            var patientId = "1";

            // Setting up User with different id as patientId
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "2"),
                new Claim(ClaimTypes.Role, "Patient")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.GetPatientInfoAsync(patientId);

            // Assert
            var forbidRes = Assert.IsType<ForbidResult>(result);
            Assert.Equal("Trying to access another patient's information.", forbidRes.AuthenticationSchemes[0]);

        }

        [Fact]
        public async Task NotFoundInDB_GetPatientInfo_ReturnsNotFound()
        {
            // Arrange (patient not seeded in db w/ id of 8)
            var patientId = "8";

            // Setting up User with same id as patientId
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "8"),
                new Claim(ClaimTypes.Role, "Patient")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.GetPatientInfoAsync(patientId);

            // Assert
            var notFoundRes = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found!", notFoundRes.Value);

        }

        // GRABBING SPECIFIC DOCTOR DETAILS

        [Fact]
        public async Task GetDoctor_ReturnsSpecificDoctor()
        {
            // Arrange
            var doctorId = "2";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, doctorId),
                new Claim(ClaimTypes.Role, "Doctor")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.GetDoctorInfoAsync(doctorId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);           // Returns Ok
            var doctorInfo = Assert.IsType<GetDoctorInfoResponse>(okResult.Value);   // OkResult obj contains doctor info
            
            var doctor = Assert.IsType<UserDto>(doctorInfo.Doctor);
            var timeSlots = Assert.IsType<List<TimeSlotDto>>(doctorInfo.TimeSlots);

            // Assert doctor info is correct
            Assert.Equal(2, doctor.Id);
            Assert.Equal("Doctor1", doctor.FullName);
            Assert.Equal("doctor1@example.com", doctor.Email);
            Assert.Equal(UserRole.Doctor.ToString(), doctor.Role);

            Assert.Equal(2, timeSlots.Count);
        } 
        
        [Fact]
        public async Task AsUnauthenticatedUser_GetDoctorInfo_ReturnsBadRequest()
        {
            // Arrange (do not setup user)
            var doctorId = "2";

            // Act
            var result = await _controller.GetDoctorInfoAsync(doctorId);

            // Assert
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("Object reference not set to an instance of an object.", badRequestRes.Value);

        }

        [Fact]
        public async Task WithoutUserId_GetDoctorInfo_ReturnsBadRequest()
        {
            // Arrange
            var doctorId = "1";

            // Setting up User without NameIdentifier claim (no user id)
            var claims = new List<Claim> { };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };


            // Act
            var result = await _controller.GetDoctorInfoAsync(doctorId);

            // Assert
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("User Id Not Defined!", badRequestRes.Value);
        }

        [Fact]
        public async Task AsAnotherDoctor_GetDoctorInfo_ReturnsForbid()
        {
            // Arrange
            var doctorId = "2";

            // Setting up User with different id as patientId
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "4"),
                new Claim(ClaimTypes.Role, "Doctor")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.GetDoctorInfoAsync(doctorId);

            // Assert
            var forbidRes = Assert.IsType<ForbidResult>(result);
            Assert.Equal("Trying to access another doctor's information.", forbidRes.AuthenticationSchemes[0]);

        }

        [Fact]
        public async Task NotFoundInDB_GetDoctorInfo_ReturnsNotFound()
        {
            // Arrange (doctor not seeded in db w/ id of 8)
            var doctorId = "8";

            // Setting up User with same id as patientId
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "8"),
                new Claim(ClaimTypes.Role, "Patient")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.GetDoctorInfoAsync(doctorId);

            // Assert
            var notFoundRes = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found!", notFoundRes.Value);

        }
    }
}
