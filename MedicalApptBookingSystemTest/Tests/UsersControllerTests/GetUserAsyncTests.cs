using MedicalApptBookingSystem.Controllers;
using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.Util;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalApptBookingSystemTest.Tests.UsersControllerTests
{
    public class GetUserAsyncTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ConvertToDto _convertToDto;
        private readonly UsersController _controller;

        public GetUserAsyncTests()
        {
            _context = TestDbContextFactory.Create();
            _convertToDto = new ConvertToDto();
            _controller = new UsersController(_context, _convertToDto);
        }

        [Fact]
        public async Task GetExistingUser_ReturnsUser()
        {
            // Arrange -- Set patientId to 1
            var patientId = 1;

            // Act -- Pass patientId to controller
            var result = await _controller.GetUserAsync(patientId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);       // Returns Ok
            var users = Assert.IsType<UserDto>(okResult.Value);         // OkResult obj contains UsersDto
        }

        [Fact]
        public async Task GetNonexistingUser_ReturnsNotFound()
        {
            // Arrange -- Set patientId to 999 (No patient w/ that id in the fake db)
            var patientId = 999;

            // Act -- Pass patientId to controller
            var result = await _controller.GetUserAsync(patientId);

            // Assert -- Returns NotFound
            var notFoundRes = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found!", notFoundRes.Value);
        }
    }
}
