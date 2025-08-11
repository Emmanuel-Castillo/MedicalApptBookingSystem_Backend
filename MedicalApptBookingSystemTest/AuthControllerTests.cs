using MedicalApptBookingSystem.Controllers;
using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MedicalApptBookingSystemTest
{
    public class AuthControllerTests
    {

        private readonly ApplicationDbContext _context;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _context = TestDbContextFactory.Create();

            // Set up the mock AuthService
            _mockAuthService = new Mock<IAuthService>();

            // Mock the token so we know what to expect
            // Argument we pass to GenerateToken is a Mock of type User
            // So we set up GenerateToken with a mock user (which can be any user), and set it to return a token string
            _mockAuthService.Setup(s => s.GenerateToken(It.IsAny<User>()))
                .Returns("fake-jwt-token");

            // Inject the mock AuthService into controller
            _controller = new AuthController(_context, _mockAuthService.Object);
        }

        [Fact]
        public async Task Register_WithValidUser_ReturnsOk()
        {
            // Arrange
            var dto = new UserRegisterDto
            {
                FullName = "Patient999",
                Email = "patient999@example.com",
                Password = "patient999",
                Role = "Patient"
            };

            // Act
            var result = await _controller.Register(dto);

            // Assert 
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Registration successful", okResult.Value);
        }

        [Fact]
        public async Task Register_WithInvalidFullName_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new UserRegisterDto
            {
                FullName = string.Empty,
                Email = string.Empty,
                Password = string.Empty,
                Role = string.Empty
            };

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Full name is required.", badRequestRes.Value);

        }

        [Fact]
        public async Task Register_WithInvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new UserRegisterDto
            {
                FullName = "John Doe",
                Email = "asdjkfalsdfjlasjflkjdflsd",
                Password = string.Empty,
                Role = string.Empty
            };

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid email format", badRequestRes.Value);

        }

        [Fact]
        public async Task Register_WithInvalidPassword_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new UserRegisterDto
            {
                FullName = "John Doe",
                Email = "johnDoe@gmail.com",
                Password = string.Empty,
                Role = string.Empty
            };

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Password must be at least 6 characters long.", badRequestRes.Value);

        }

        [Fact]
        public async Task Register_WithInvalidRole_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new UserRegisterDto
            {
                FullName = "John Doe",
                Email = "johnDoe@gmail.com",
                Password = "johnDoesPW",
                Role = "MadeUpRole"
            };

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid role selected", badRequestRes.Value);

        }

        [Fact]
        public async Task Register_WithExistingUser_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new UserRegisterDto
            {
                FullName = "John Doe",
                Email = "johnDoe@gmail.com",
                Password = "johnDoesPW",
                Role = "Patient"
            };
            await _controller.Register(registerDto);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert first user's registration
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Email already registered.", badRequestRes.Value);

        }

        [Fact]
        public async Task Login_WithValidUser_ReturnsOk()
        {
            // Arrange
            // Register a new user
            var registerDto = new UserRegisterDto
            {
                FullName = "Patient999",
                Email = "patient999@example.com",
                Password = "patient999",
                Role = "Patient"
            };

            var registerResult = await _controller.Register(registerDto);

            // Log user in 
            var loginDto = new UserLoginDto
            {
                Email = "patient999@example.com",
                Password = "patient999"
            };

            // Act
            var loginResult = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(loginResult);

            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            Assert.Contains("fake-jwt-token", json);

        }

        [Fact]
        public async Task Login_WithInvalidUser_ReturnsUnauthorized()
        {
            // Arrange
            // Register a new user
            var registerDto = new UserRegisterDto
            {
                FullName = "Patient1",
                Email = "patient1@example.com",
                Password = "patient1",
                Role = "Patient"
            };

            var registerResult = await _controller.Register(registerDto);

            // Log user in with invalid credentails
            var loginDto = new UserLoginDto
            {
                Email = "patient1@example.com",
                Password = "doctor1"
            };

            // Act
            var loginResult = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(loginResult);
            Assert.Equal("Invalid credentials.", unauthorizedResult.Value);

        }
    }
}
