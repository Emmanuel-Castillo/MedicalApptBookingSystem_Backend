using MedicalApptBookingSystem.Controllers;
using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.DTO.Reponses;
using MedicalApptBookingSystem.Models;
using MedicalApptBookingSystem.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicalApptBookingSystemTest.Tests.PatientControllerTests
{
    public class GetPatientAsyncTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ConvertToDto _convertToDto;
        private readonly PatientController _controller;

        public GetPatientAsyncTests()
        {
            _context = TestDbContextFactory.Create();
            _convertToDto = new ConvertToDto();
            _controller = new PatientController(_context, _convertToDto);
        }
        
        [Fact]
        public async Task AsPatient_RequestOwnPatientInfo_ReturnsOk()
        {
            // Arrange -- Set up FakePatient (Patient1) in the controller's HTTPContext
            var patientId = 1;  // Using Patient1's Id
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakePatient() }
            };

            // Act -- Pass patientId to controller
            var result = await _controller.GetPatientAsync(patientId);

            // Assert -- Returns Ok and Patient1's info
            var okResult = Assert.IsType<OkObjectResult>(result);           // Returns Ok
            var patientInfo = Assert.IsType<GetPatientInfoResponse>(okResult.Value);   // OkResult obj contains patient info
            
            var patient = Assert.IsType<UserDto>(patientInfo.Patient);
            var appointments = Assert.IsType<List<AppointmentDto>>(patientInfo.AppointmentsThisWeek);

            // Assert patient info is correct
            Assert.Equal(1, patient.Id);
            Assert.Equal("Patient1", patient.FullName);
            Assert.Equal("patient1@example.com", patient.Email);
            Assert.Equal(UserRole.Patient.ToString(), patient.Role);

            Assert.Single(appointments);
        }
        
        [Fact]
        public async Task AsUnauthenticatedUser_RequestPatientInfo_ReturnsBadRequest()
        {
            // Arrange (do not setup user)
            var patientId = 1;

            // Act -- Pass patientId to controller
            var result = await _controller.GetPatientAsync(patientId);

            // Assert -- Returns BadRequest
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Object reference not set to an instance of an object.", badRequestRes.Value);

        }

        [Fact]
        public async Task UsingInvalidUserIdClaim_RequestPatientInfo_ReturnsBadRequest()
        {
            // Arrange -- Setup authenticated User w/out any claims into controller's HTTP context
            var patientId = 1;

            // Setting up User without NameIdentifier claim (no user id)
            var claims = new List<Claim> { };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act -- Pass patientId to controller
            var result = await _controller.GetPatientAsync(patientId);

            // Assert -- Returns BadRequest
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("User Id Not Defined!", badRequestRes.Value);

        }

        [Fact]
        public async Task AsAnotherPatient_RequestPatientInfo_ReturnsForbid()
        {
            // Arrange -- Setup authenticated User as different than the requested User
            var patientId = 1;

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

            // Act -- Pass patientId to controller
            var result = await _controller.GetPatientAsync(patientId);

            // Assert -- Returns Forbid
            var forbidRes = Assert.IsType<ForbidResult>(result);
            Assert.Equal("Trying to access another patient's information.", forbidRes.AuthenticationSchemes[0]);
        }

        [Fact]
        public async Task RequestPatientInfo_NotFoundInDB_ReturnsNotFound()
        {
            // Arrange (patient not seeded in db w/ id of 8)
            var patientId = 8;

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
            var result = await _controller.GetPatientAsync(patientId);

            // Assert
            var notFoundRes = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found!", notFoundRes.Value);

        }
    }
}
