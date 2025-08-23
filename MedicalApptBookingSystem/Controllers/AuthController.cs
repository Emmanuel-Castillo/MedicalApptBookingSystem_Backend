 
using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.Models;
using MedicalApptBookingSystem.Services;
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
        private readonly IAuthService _authService;

        public AuthController(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
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
                if (user == null || user.PasswordHash != HashPassword(dto.Password))
                {
                    return Unauthorized("Invalid credentials.");
                }

                // Once credentials are verified, generate JWT token and return back to user for user authentication
                var token = _authService.GenerateToken(user);
                var res = new { token };
                return Ok(new { token });
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
            
        }

        private string HashPassword(string password) {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
