using MedicalApptBookingSystem.Controllers;
using MedicalApptBookingSystem.Data;
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
    public class CancelAppointmentAsyncTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ConvertToDto _convertToDto;
        private readonly AppointmentsController _controller;

        public CancelAppointmentAsyncTests()
        {
            _context = TestDbContextFactory.Create();
            _convertToDto = new ConvertToDto();
            _controller = new AppointmentsController(_context, _convertToDto);
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

            // Act
            // Fake Patient (Patient 1) is booked with appt id = 1
            var response = await _controller.CancelAppointmentAsync(1);

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

            // Act
            // Appt with id = 5 not included in seeded fake db
            var response = await _controller.CancelAppointmentAsync(5);

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

            // Act
            // Fake Patient (Patient 1) is booked with appt id = 1
            var response = await _controller.CancelAppointmentAsync(1);

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

            // Act
            // Appt with id = 5 not included in seeded fake db
            var response = await _controller.CancelAppointmentAsync(5);

            // Assert
            var notFoundRes = Assert.IsType<NotFoundObjectResult>(response);
            Assert.Equal("Appointment not found or not owned by the current user.", notFoundRes.Value);
        }
    }
}
