using MedicalApptBookingSystem.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalApptBookingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeepAliveController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public KeepAliveController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> KeepAlive()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                return Ok("Alive");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
