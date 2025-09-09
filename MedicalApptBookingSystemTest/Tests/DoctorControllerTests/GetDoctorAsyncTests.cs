using MedicalApptBookingSystem.Controllers;
using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.DTO.Reponses;
using MedicalApptBookingSystem.Models;
using MedicalApptBookingSystem.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MedicalApptBookingSystemTest.Tests.DoctorControllerTests
{
    public class GetDoctorAsyncTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ConvertToDto _convertToDto;
        private readonly DoctorController _controller;

        public GetDoctorAsyncTests()
        {
            _context = TestDbContextFactory.Create();
            _convertToDto = new ConvertToDto();
            _controller = new DoctorController(_context, _convertToDto);
        }

        [Fact]
        public async Task GetDoctorAsync_ReturnsSpecificDoctor()
        {
            // Arrange
            var doctorId = 2;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = HTTPUserContext.GetFakeDoctor() }
            };

            // Act
            var result = await _controller.GetDoctorAsync(doctorId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);           // Returns Ok
            var doctorInfo = Assert.IsType<GetDoctorInfoResponse>(okResult.Value);   // OkResult obj contains doctor info
            
            var doctorProfile = Assert.IsType<DoctorDto>(doctorInfo.DoctorProfile);
            var timeSlots = Assert.IsType<List<TimeSlotDto>>(doctorInfo.BookedTimeSlotsNextTwoWeeks);

            var doctor = Assert.IsType<UserDto>(doctorProfile.User);

            // Assert doctor info is correct
            Assert.Equal(2, doctor.Id);
            Assert.Equal("Doctor1", doctor.FullName);
            Assert.Equal("doctor1@example.com", doctor.Email);
            Assert.Equal(UserRole.Doctor.ToString(), doctor.Role);

            Assert.Empty(timeSlots);
        } 
        
        [Fact]
        public async Task GetDoctorAsync_AsUnauthenticatedUser_ReturnsBadRequest()
        {
            // Arrange (do not setup user)
            var doctorId = 2;

            // Act
            var result = await _controller.GetDoctorAsync(doctorId);

            // Assert
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("Object reference not set to an instance of an object.", badRequestRes.Value);

        }

        [Fact]
        public async Task GetDoctorAsync_WithoutUserId_ReturnsBadRequest()
        {
            // Arrange
            var doctorId = 1;

            // Setting up User without NameIdentifier claim (no user id)
            var claims = new List<Claim> { };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };


            // Act
            var result = await _controller.GetDoctorAsync(doctorId);

            // Assert
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("User Id Not Defined!", badRequestRes.Value);
        }

        [Fact]
        public async Task GetDoctorAsync_AsAnotherDoctor_ReturnsForbid()
        {
            // Arrange
            var doctorId = 2;

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
            var result = await _controller.GetDoctorAsync(doctorId);

            // Assert
            var forbidRes = Assert.IsType<ForbidResult>(result);
            Assert.Equal("Trying to access another doctor's information.", forbidRes.AuthenticationSchemes[0]);

        }

        [Fact]
        public async Task GetDoctorAsync_NotFoundInDB_ReturnsNotFound()
        {
            // Arrange (doctor not seeded in db w/ id of 8)
            var doctorId = 8;

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
            var result = await _controller.GetDoctorAsync(doctorId);

            // Assert
            var notFoundRes = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found!", notFoundRes.Value);

        }
    }
}
