using MedicalApptBookingSystem.Controllers;
using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.Util;
using Microsoft.AspNetCore.Mvc;

namespace MedicalApptBookingSystemTest.Tests.PatientControllerTests
{
    public class GetAllPatientsAsyncTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ConvertToDto _convertToDto;
        private readonly PatientController _controller;

        public GetAllPatientsAsyncTests()
        {
            _context = TestDbContextFactory.Create();
            _convertToDto = new ConvertToDto();
            _controller = new PatientController(_context, _convertToDto);
        }

        [Fact]
        public async Task GetAllPatientsAsync_ReturnsAllPatients()
        {
            // Act
            var result = await _controller.GetAllPatientsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);           // Returns Ok
            var patients = Assert.IsType<List<PatientDto>>(okResult.Value);   // OkResult obj contains list of UsersDto
            Assert.Equal(2, patients.Count());                             // There are only 2 in the list
        }
    }
}
