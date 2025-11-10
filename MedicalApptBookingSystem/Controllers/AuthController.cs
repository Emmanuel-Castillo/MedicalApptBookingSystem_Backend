 
using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.DTO.Requests;
using MedicalApptBookingSystem.Models;
using MedicalApptBookingSystem.Services;
using MedicalApptBookingSystem.Util;
using MedicalApptBookingSystemTest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MedicalApptBookingSystem.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ConvertToDto _convertToDto;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public AuthController(ApplicationDbContext context, IAuthService authService, IEmailService emailService, ConvertToDto convertToDto)
        {
            _context = context;
            _authService = authService;
            _emailService = emailService;
            _convertToDto = convertToDto;
        }

        // Endpoint accessible for all Users
        // Create a new User account
        // Hashes password using utility function below and saves it to db
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            try { 
                // Validate dto
                if (string.IsNullOrEmpty(dto.FullName))
                    return BadRequest("Full name is required.");

                if (!Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    return BadRequest("Invalid email format");

                if (dto.Password.Length < 6)
                    return BadRequest("Password must be at least 6 characters long.");

                if (!Enum.TryParse<UserRole>(dto.Role, true, out var parsedRole))
                    return BadRequest("Invalid role selected");

                // Existing user contains same email, return BadRequest
                if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                    return BadRequest("Email already registered.");

                // Create new User and save to db
                var user = new User
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PasswordHash = HashPassword(dto.Password),
                    Role = Enum.Parse<UserRole>(dto.Role, true)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok("Registration successful");
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }
        }


        // Endpoint accessible for all Users
        // Log in to their account, retrieving their auth token
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            try { 
                // Check if user credentials exist in the db, and if both email and password match
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null) return NotFound($"User with email ({dto.Email}) doesn't exist.");
                if (user.PasswordHash != HashPassword(dto.Password))
                {
                    return Unauthorized("Invalid credentials.");
                }

                // Once credentials are verified, generate JWT token and return back to user for user authentication
                var token = _authService.GenerateToken(user);
                var userDto = _convertToDto.ConvertToUserDto(user);
                return Ok(new { token, userDto });
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null) return NotFound("User not found.");

                var resetToken = GenerateUrlSafeToken();
                user.PasswordResetToken = resetToken;
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

                await _context.SaveChangesAsync();

                await _emailService.SendEmailAsync(user.Email,
                    "Reset your password",
                    $"Click here to reset: http://localhost:3000/reset-password?token={resetToken}");

                return Ok("If that email exists, we've sent a reset link");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest dto)
        {
            try {
                var user = await _context.Users.FirstOrDefaultAsync(u =>
                    u.PasswordResetToken == dto.Token && u.PasswordResetTokenExpiry > DateTime.UtcNow
                );

                if (user == null) return BadRequest("Invalid or expired token.");

                user.PasswordHash = HashPassword(dto.NewPassword);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;

                await _context.SaveChangesAsync();

                return Ok("Password reset successful."); 
            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        private string GenerateUrlSafeToken()
        {
            var bytes = Guid.NewGuid().ToByteArray();
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        private string HashPassword(string password) {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
