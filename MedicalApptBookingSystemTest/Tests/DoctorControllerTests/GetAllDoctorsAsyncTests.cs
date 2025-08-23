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

namespace MedicalApptBookingSystemTest.Tests.DoctorControllerTests
{
    public class GetAllDoctorsAsyncTests
    {

        private readonly ApplicationDbContext _context;
        private readonly ConvertToDto _convertToDto;
        private readonly DoctorController _controller;

        public GetAllDoctorsAsyncTests()
        {
            _context = TestDbContextFactory.Create();
            _convertToDto = new ConvertToDto();
            _controller = new DoctorController(_context, _convertToDto);
        }

        [Fact]
        public async Task GetAllDoctorsAsync_ReturnsAllDoctors()
        {
            // Act
            var result = await _controller.GetAllDoctorsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);           // Returns Ok
            var doctors = Assert.IsType<List<UserDto>>(okResult.Value);   // OkResult obj contains list of UsersDto
            Assert.Equal(2, doctors.Count());                             // There are only 2 in the list
        }
    }
}
