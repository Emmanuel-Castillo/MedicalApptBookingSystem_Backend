using MedicalApptBookingSystem.Controllers;
using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.Models;
using MedicalApptBookingSystem.Services;
using MedicalApptBookingSystem.Util;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MedicalApptBookingSystemTest.Tests.AuthControllerTests
{
    public class LoginTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;
        private readonly ConvertToDto _convertToDto;

        public LoginTests()
        {
            // _context already contains seeded User data
            _context = TestDbContextFactory.Create();
            _convertToDto = new ConvertToDto();

            // Set up the mock AuthService
            _mockAuthService = new Mock<IAuthService>();
            var _emailService = new Mock<IEmailService>();


            // Mock the token so we know what to expect
            // Argument we pass to GenerateToken is a Mock of type User
            // So we set up GenerateToken with a mock user (which can be any user), and set it to return a token string
            _mockAuthService.Setup(s => s.GenerateToken(It.IsAny<User>()))
                .Returns("fake-jwt-token");


            // Inject the mock AuthService into controller
            _controller = new AuthController(_context, _mockAuthService.Object, _emailService.Object, _convertToDto);
        }
        
        [Fact]
        public async Task Login_WithValidUser_ReturnsOk()
        {
            // Arrange
            // Creating new User using credentials below
            // Email below is not being used by any User in our fake db, so registration should be accepted
            var email = "patient999@example.com";
            var password = "patient999";

            // Register a new user
            var registerDto = new UserRegisterDto
            {
                FullName = "Patient999",
                Email = email,
                Password = password,
                Role = "Patient"
            };
            var registerResult = await _controller.Register(registerDto);

            // Set up UserLoginDto
            var loginDto = new UserLoginDto
            {
                Email = email,
                Password = password
            };

            // Act -- Pass UserLoginDto to controller
            var loginResult = await _controller.Login(loginDto);

            // Assert -- Returns OkResult
            var okResult = Assert.IsType<OkObjectResult>(loginResult);
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            Assert.Contains("fake-jwt-token", json);

        }

        [Fact]
        public async Task Login_WithInvalidUser_ReturnsUnauthorized()
        {
            // Arrange -- Set up Dto using invalid credentails
            // Attempt to login as User "Patient1" from db 
            var loginDto = new UserLoginDto
            {
                Email = "patient1@example.com",
                Password = "doctor1"
            };

            // Act -- Pass Dto to controller
            var loginResult = await _controller.Login(loginDto);

            // Assert -- Return UnauthorizedResult
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(loginResult);
            Assert.Equal("Invalid credentials.", unauthorizedResult.Value);

        }
    }
}
