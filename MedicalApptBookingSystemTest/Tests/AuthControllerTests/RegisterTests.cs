using MedicalApptBookingSystem.Controllers;
using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MedicalApptBookingSystemTest.Tests.AuthControllerTests
{
    public class RegisterTests
    {

        private readonly ApplicationDbContext _context;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public RegisterTests()
        {
            _context = TestDbContextFactory.Create();
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_context, _mockAuthService.Object);
        }

        [Fact]
        public async Task Register_WithValidUser_ReturnsOk()
        {
            // Arrange -- Set up Dto with valid credentials
            var dto = new UserRegisterDto
            {
                FullName = "Patient999",
                Email = "patient999@example.com",
                Password = "patient999",
                Role = "Patient"
            };

            // Act -- Pass Dto to controller
            var result = await _controller.Register(dto);

            // Assert -- Returns OkResult
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Registration successful", okResult.Value);
        }

        [Fact]
        public async Task Register_WithInvalidFullName_ReturnsBadRequest()
        {
            // Arrange -- Set up Dto using invalid full name
            var registerDto = new UserRegisterDto
            {
                FullName = string.Empty,
                Email = string.Empty,
                Password = string.Empty,
                Role = string.Empty
            };

            // Act -- Pass Dto to controller
            var result = await _controller.Register(registerDto);

            // Assert -- Returns BadRequest
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Full name is required.", badRequestRes.Value);

        }

        [Fact]
        public async Task Register_WithInvalidEmail_ReturnsBadRequest()
        {
            // Arrange -- Set up Dto using invalid email
            var registerDto = new UserRegisterDto
            {
                FullName = "John Doe",
                Email = "asdjkfalsdfjlasjflkjdflsd",
                Password = string.Empty,
                Role = string.Empty
            };

            // Act -- Pass Dto to controller
            var result = await _controller.Register(registerDto);

            // Assert -- Returns BadRequest
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid email format", badRequestRes.Value);

        }

        [Fact]
        public async Task Register_WithInvalidPassword_ReturnsBadRequest()
        {
            // Arrange -- Set up Dto with invalid password
            var registerDto = new UserRegisterDto
            {
                FullName = "John Doe",
                Email = "johnDoe@gmail.com",
                Password = string.Empty,
                Role = string.Empty
            };

            // Act -- Pass Dto to controller
            var result = await _controller.Register(registerDto);

            // Assert -- Returns BadRequest
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Password must be at least 6 characters long.", badRequestRes.Value);

        }

        [Fact]
        public async Task Register_WithInvalidRole_ReturnsBadRequest()
        {
            // Arrange -- Set up Dto using invalid Role
            var registerDto = new UserRegisterDto
            {
                FullName = "John Doe",
                Email = "johnDoe@gmail.com",
                Password = "johnDoesPW",
                Role = "MadeUpRole"
            };

            // Act -- Pass Dto to controller
            var result = await _controller.Register(registerDto);

            // Assert -- Retuns BadRequest
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid role selected", badRequestRes.Value);

        }

        [Fact]
        public async Task Register_WithExistingUser_ReturnsBadRequest()
        {
            // Arrange -- Register a new User
            var registerDto = new UserRegisterDto
            {
                FullName = "John Doe",
                Email = "johnDoe@gmail.com",
                Password = "johnDoesPW",
                Role = "Patient"
            };
            await _controller.Register(registerDto);

            // Act -- Register using the same credentials
            var result = await _controller.Register(registerDto);

            // Assert -- Must return BadRequest
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Email already registered.", badRequestRes.Value);

        }

        
    }
}
