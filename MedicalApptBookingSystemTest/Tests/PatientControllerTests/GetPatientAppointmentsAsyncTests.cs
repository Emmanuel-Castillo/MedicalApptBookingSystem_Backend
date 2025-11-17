using MedicalApptBookingSystem.Controllers;
using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.DTO.Reponses;
using MedicalApptBookingSystem.DTO.Requests;
using MedicalApptBookingSystem.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MedicalApptBookingSystemTest.Tests.PatientControllerTests
{
    public class GetPatientAppointmentsAsyncTests
    {

        private readonly ApplicationDbContext _context;
        private readonly ConvertToDto _convertToDto;
        private readonly PatientController _controller;

        public GetPatientAppointmentsAsyncTests()
        {
            // Setup test db context
            _context = TestDbContextFactory.Create();

            // Set up the mock AuthService
            _convertToDto = new ConvertToDto();

            // Inject the mock AuthService into controller
            _controller = new PatientController(_context, _convertToDto);
        }
        [Fact]
        public async Task AsPatient_GetOwnAppointments_ReturnsOkRes()
        {
            // Arrange -- Setting up fake Patient for HTTP Context
            var patientId = 1;
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakePatient() }
            };

            // Act -- Pass patientId to controller
            var response = await _controller.GetPatientAppointmentsAsync(patientId);

            // Assert -- Returns OkObjectResult and response obj
            // Patient1 only has 1 apppointment booked
            var okResult = Assert.IsType<OkObjectResult>(response);
            var responseObj = Assert.IsType<GetPatientAppointmentsResponse>(okResult.Value);

            var apptList = Assert.IsType<List<AppointmentDto>>(responseObj.Appointments);
            Assert.Single(apptList);

            var totalCount = Assert.IsType<int>(responseObj.TotalCount);
            Assert.Equal(1, totalCount);
        }

        [Fact]
        public async Task AsPatient_AttemptGrabbingAnotherPatientsAppointments_ReturnsForbid()
        {
            // Arrange -- Set up fake User (Patient1) to HTTP Context
            // This patientId will be different from the fake User's Id (1)
            var patientId = 999;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakePatient() }
            };

            // Act -- Pass patientId to controller
            var response = await _controller.GetPatientAppointmentsAsync(patientId);

            // Assert -- Return Forbid

            // UPDATE: RETURNS NOT FOUND WITH INCLUSION OF PATIENT MODEL
            Assert.IsType<NotFoundObjectResult>(response);
            //var forbidRes = Assert.IsType<ForbidResult>(response);
            //Assert.Equal("Attempting to access another patient's appointments.", forbidRes.AuthenticationSchemes[0]);
        }

        [Fact]
        public async Task AsAdmin_GetSomeonesAppointment_ReturnsOkRes()
        {
            // Arrange -- Set up authorized User as Admin for HTTP Context
            var patientId = 1;
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakeAdmin() }
            };
            
            // Act
            var response = await _controller.GetPatientAppointmentsAsync(patientId);

            // Assert -- Returns OkObjectResult and response obj
            // Patient1 only has 1 apppointment booked
            var okResult = Assert.IsType<OkObjectResult>(response);
            var responseObj = Assert.IsType<GetPatientAppointmentsResponse>(okResult.Value);

            var apptList = Assert.IsType<List<AppointmentDto>>(responseObj.Appointments);
            Assert.Single(apptList);

            var totalCount = Assert.IsType<int>(responseObj.TotalCount);
            Assert.Equal(1, totalCount);
        }

        [Fact]
        public async Task GettingAppointmentsFromNonexistingPatient_ReturnsNotFound()
        {
            // Arrange -- Set patientId to one not saved in db
            var patientId = 999;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "999"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act -- Pass patientId to controller
            var response = await _controller.GetPatientAppointmentsAsync(patientId);

            // Assert -- Returns OkResult, empty list of AppointmentDto, and TotalCount = 0
            var okResult = Assert.IsType<NotFoundObjectResult>(response);
            //var responseObj = Assert.IsType<GetPatientAppointmentsResponse>(okResult.Value);

            //var apptList = Assert.IsType<List<AppointmentDto>>(responseObj.Appointments);
            //Assert.Empty(apptList);

            //var totalCount = Assert.IsType<int>(responseObj.TotalCount);
            //Assert.Equal(0, totalCount);
        }
    }
}
