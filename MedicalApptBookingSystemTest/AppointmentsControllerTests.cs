using MedicalApptBookingSystem.Controllers;
using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
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

namespace MedicalApptBookingSystemTest
{
    public class AppointmentsControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ConvertToDto _convertToDto;
        private readonly AppointmentsController _controller;

        public AppointmentsControllerTests()
        {
            // Setup test db context
            _context = TestDbContextFactory.Create();

            // Set up the mock AuthService
            _convertToDto = new ConvertToDto();

            // Inject the mock AuthService into controller
            _controller = new AppointmentsController(_context, _convertToDto);
        }

        // GETTING SINGLE APPOINMENT AS PATIENT

        [Fact]
        public async Task AsPatient_GetAppointment_ReturnsOkRes()
        {
            // Arrange
            // Setting up fake Patient for HTTP Context
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakePatient() }
            };

            var request = new GetPatientAppointmentsRequest { };

            // Act
            var response = await _controller.GetAppointment(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            var apptList = Assert.IsType<AppointmentDto>(okResult.Value);

        }

        [Fact]
        public async Task AsPatient_GetAnotherPatientsAppt_ReturnsForbid()
        {
            // Arrange
            // Setting up fake Patient for HTTP Context
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakePatient() }
            };

            // Act
            // Appt w/ Id = 2 is booked by a different patient.
            var response = await _controller.GetAppointment(2);

            // Assert
            var forbidResult = Assert.IsType<ForbidResult>(response);
            Assert.Equal("Trying to access another patient's appointment.", forbidResult.AuthenticationSchemes[0]);
        }

        [Fact]
        public async Task AsPatient_GetNonexistingAppointment_ReturnsNotFound()
        {
            // Arrange
            // Setting up fake Patient for HTTP Context
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakePatient() }
            };

            // Act
            // Using id = 3, since there are no appts in the db w/ id = 3
            var response = await _controller.GetAppointment(3);

            // Assert
            var notFoundRes = Assert.IsType<NotFoundObjectResult>(response);
            Assert.Equal("Appointment not found.", notFoundRes.Value);
        }

        // GETTING SINGLE APPOINTMENT AS ADMIN

        [Fact]
        public async Task AsAdmin_GetPatientAppointment_ReturnsOkRes()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakeAdmin() }
            };

            // Act
            var response = await _controller.GetAppointment(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            var apptList = Assert.IsType<AppointmentDto>(okResult.Value);
        }

        [Fact]
        public async Task AsAdmin_GetNonexistingAppointment_ReturnsNotFound()
        {
            // Arrange
            // Setting up fake Patient for HTTP Context
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakeAdmin() }
            };

            // Act
            // Using id = 3, since there are no appts in the db w/ id = 3
            var response = await _controller.GetAppointment(3);

            // Assert
            var notFoundRes = Assert.IsType<NotFoundObjectResult>(response);
            Assert.Equal("Appointment not found.", notFoundRes.Value);
        }

        // GETTING LIST OF APPOINTMENTS AS PATIENT OR ADMIN

        [Fact]
        public async Task AsPatient_GetOwnAppointments_ReturnsOkRes()
        {
            // Arrange
            // Setting up fake Patient for HTTP Context
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakePatient() }
            };

            var request = new GetPatientAppointmentsRequest { };

            // Act
            var response = await _controller.GetAppointments(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            var apptList = Assert.IsType<List<AppointmentDto>>(okResult.Value);
            Assert.Single(apptList);
        }

        [Fact]
        public async Task AsPatient_GetAnotherPatientsAppointments_ReturnsForbid()
        {
            // Setting up own Patient User claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Patient")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Request will include another patient's Id
            var request = new GetPatientAppointmentsRequest { 
                PatientId = "3"
            };

            // Act
            var response = await _controller.GetAppointments(request);

            // Assert
            var forbidRes = Assert.IsType<ForbidResult>(response);
            Assert.Equal("Attempting to retrieve a Patient's appointment, but the current auth User is not an Admin.", forbidRes.AuthenticationSchemes[0]);
        }

        [Fact]
        public async Task AsAdmin_GetSomeonesAppointment_ReturnsOkRes()
        {
            // Arrange
            var patientId = "1";

            // Setup Admin User
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
            
            // Setup request using patientId
            var request = new GetPatientAppointmentsRequest {
                PatientId = patientId
            };

            // Act
            var response = await _controller.GetAppointments(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            var apptList = Assert.IsType<List<AppointmentDto>>(okResult.Value);
            Assert.Single(apptList);
        }

        // BOOKING APPT AS PATIENT

        [Fact]
        public async Task AsPatient_BookAppointment_ReturnsOkRes() {

            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakePatient() }
            };

            // Setting an appointment request for second time slot in the seeded list
            var request = new BookAppointmentsRequest
            {
                TimeSlotId = 3
            };

            // Act
            var response = await _controller.BookAppointment(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal("Appointment booked successfully.", okResult.Value);

        }

        [Fact]
        public async Task AsPatient_BookApptAtBookedTimeSlot_ReturnsBadRequest() {

            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakePatient() }
            };

            // Setting an appointment request for first time slot, which is already booked
            var request = new BookAppointmentsRequest
            {
                TimeSlotId = 1
            };

            // Act
            var response = await _controller.BookAppointment(request);

            // Assert
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal("Invalid or already booked time slot.", badRequestRes.Value);
        }

        [Fact]
        public async Task AsPatient_BookAnotherPatientsAppointment_ReturnsForbid()
        {
            // Setting up own Patient User claims
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakePatient() }
            };

            // Request will include another patient's Id
            var request = new BookAppointmentsRequest
            {
                PatientId = "3",
                TimeSlotId = 2
            };

            // Act
            var response = await _controller.BookAppointment(request);

            // Assert
            var forbidRes = Assert.IsType<ForbidResult>(response);
            Assert.Equal("Attempting to book a Patient's appointment, but the current auth User is not an Admin.", forbidRes.AuthenticationSchemes[0]);
        }

        // BOOKING APPT AS ADMIN

        [Fact]
        public async Task AsAdmin_BookAppointment_ReturnsOkRes() {

            // Arrange
            var patientId = "1";

            // Setup Admin User
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakeAdmin() }
            };

            // Setting an appointment request for second time slot, as well as patientId
            var request = new BookAppointmentsRequest
            {
                PatientId = patientId,
                TimeSlotId = 3
            };

            // Act
            var response = await _controller.BookAppointment(request);

            // Assert
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal("Appointment booked successfully.", okResult.Value);
        }

        [Fact]
        public async Task AsAdmin_BookApptAtBookedTimeSlot_ReturnsBadRequest()
        {
            // Arrange
            var patientId = "1";

            // Setup Admin User
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

            // Setting an appointment request for first time slot, which is already booked
            var request = new BookAppointmentsRequest
            {
                PatientId = patientId,
                TimeSlotId = 1
            };

            // Act
            var response = await _controller.BookAppointment(request);

            // Assert
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal("Invalid or already booked time slot.", badRequestRes.Value);
        }

        // DELETING APPT AS PATIENT

        [Fact]
        public async Task AsPatient_CancelAppointment_ReturnsOkRes()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakePatient() }
            };

            // Fake Patient (Patient 1) is booked with appt id = 1
            var request = new DeleteAppointmentRequest
            {
                AppointmentId = 1
            };

            // Act
            var response = await _controller.CancelAppointment(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal("Appointment successfully cancelled.", okResult.Value);
        }

        [Fact]
        public async Task AsPatient_CancelNonexistingAppt_ReturnsNotFound()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakePatient() }
            };

            // Appt with id = 5 not included in seeded fake db
            var request = new DeleteAppointmentRequest
            {
                AppointmentId = 5
            };

            // Act
            var response = await _controller.CancelAppointment(request);

            // Assert
            var notFoundRes = Assert.IsType<NotFoundObjectResult>(response);
            Assert.Equal("Appointment not found or not owned by the current user.", notFoundRes.Value);
        }

        // DELETING APPT AS ADMIN

        [Fact]
        public async Task AsAdmin_CancelAppointment_ReturnsOkRes()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakeAdmin() }
            };

            // Fake Patient (Patient 1) is booked with appt id = 1
            var request = new DeleteAppointmentRequest
            {
                AppointmentId = 1
            };

            // Act
            var response = await _controller.CancelAppointment(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal("Appointment successfully cancelled.", okResult.Value);
        }

        [Fact]
        public async Task AsAdmin_CancelNonexistingAppt_ReturnsNotFound()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakeAdmin() }
            };

            // Appt with id = 5 not included in seeded fake db
            var request = new DeleteAppointmentRequest
            {
                AppointmentId = 5
            };

            // Act
            var response = await _controller.CancelAppointment(request);

            // Assert
            var notFoundRes = Assert.IsType<NotFoundObjectResult>(response);
            Assert.Equal("Appointment not found or not owned by the current user.", notFoundRes.Value);
        }
    }
}
