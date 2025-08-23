using MedicalApptBookingSystem.Controllers;
using MedicalApptBookingSystem.Data;
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

namespace MedicalApptBookingSystemTest.Tests.AppointmentControllerTests
{
    public class BookAppointmentAsyncTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ConvertToDto _convertToDto;
        private readonly AppointmentsController _controller;

        public BookAppointmentAsyncTests()
        {
            _context = TestDbContextFactory.Create();
            _convertToDto = new ConvertToDto();
            _controller = new AppointmentsController(_context, _convertToDto);
        }

        // BOOKING APPT AS PATIENT

        [Fact]
        public async Task AsPatient_BookAppointment_ReturnsOkRes()
        {

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
            var response = await _controller.BookAppointmentAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal("Appointment booked successfully.", okResult.Value);

        }

        [Fact]
        public async Task AsPatient_BookApptAtBookedTimeSlot_ReturnsBadRequest()
        {

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
            var response = await _controller.BookAppointmentAsync(request);

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
            var response = await _controller.BookAppointmentAsync(request);

            // Assert
            var forbidRes = Assert.IsType<ForbidResult>(response);
            Assert.Equal("Attempting to book a Patient's appointment, but the current auth User is not an Admin.", forbidRes.AuthenticationSchemes[0]);
        }

        // BOOKING APPT AS ADMIN

        [Fact]
        public async Task AsAdmin_BookAppointment_ReturnsOkRes()
        {

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
            var response = await _controller.BookAppointmentAsync(request);

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
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakeAdmin() }
            };

            // Setting an appointment request for first time slot, which is already booked
            var request = new BookAppointmentsRequest
            {
                PatientId = patientId,
                TimeSlotId = 1
            };

            // Act
            var response = await _controller.BookAppointmentAsync(request);

            // Assert
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal("Invalid or already booked time slot.", badRequestRes.Value);
        }
    }
}
