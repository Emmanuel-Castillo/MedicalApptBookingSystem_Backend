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
using System.Text;
using System.Threading.Tasks;

namespace MedicalApptBookingSystemTest.Tests.AppointmentControllerTests
{
    public class GetAppointmentAsyncTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ConvertToDto _convertToDto;
        private readonly AppointmentsController _controller;

        public GetAppointmentAsyncTests()
        {
            _context = TestDbContextFactory.Create();
            _convertToDto = new ConvertToDto();
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
            var response = await _controller.GetAppointmentAsync(1);

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
            var response = await _controller.GetAppointmentAsync(2);

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
            var response = await _controller.GetAppointmentAsync(3);

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
            var response = await _controller.GetAppointmentAsync(1);

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
            var response = await _controller.GetAppointmentAsync(3);

            // Assert
            var notFoundRes = Assert.IsType<NotFoundObjectResult>(response);
            Assert.Equal("Appointment not found.", notFoundRes.Value);
        }
    }
}
