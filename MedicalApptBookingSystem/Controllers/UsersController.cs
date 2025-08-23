using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO.Requests;
using MedicalApptBookingSystem.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace MedicalApptBookingSystem.Controllers
{
    [Route("users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        ApplicationDbContext _context;
        ConvertToDto _convertToDto;
        public UsersController(ApplicationDbContext context, ConvertToDto convertToDto)
        {
            _context = context;
            _convertToDto = convertToDto;
        }

        // Endpoint authorized for Admins ONLY
        // Retrieve ALL Users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                var usersDto = _convertToDto.ConvertToListUserDto(users);
                return Ok(usersDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        // Endpoint accessible to all authorized Users
        // Retrieve specific User
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserAsync(int id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null) return NotFound("User not found!");

                var userDto = _convertToDto.ConvertToUserDto(user);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoint accessible for Admins ONLY
        // Edit a specific User's data
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUserAsync(ChangeUserRequest request)
        {
            try
            {
                // Check if valid email
                if (!Regex.IsMatch(request.NewEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    return BadRequest("Invalid email format");

                // Find user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id);
                if (user == null) return NotFound("User not found!");

                // Check if request NewEmail has been taken by another User!
                var userWithNewEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.NewEmail);
                if (userWithNewEmail != null) return BadRequest("This requested new email has already been taken!");

                user.FullName = request.NewFullName;
                user.Email = request.NewEmail;

                await _context.SaveChangesAsync();
                return Ok("User has been updated!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
