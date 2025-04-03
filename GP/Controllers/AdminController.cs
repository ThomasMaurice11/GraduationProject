using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GP.DTOs;
using GP.Models;
using GP.DTOs.Admin;

namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public AdminController(AuthDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetRegistarationRequests")]
        public async Task<IActionResult> GetRegistarationRequests()
        {
            var requests = await _context.Doctors
                .Where(p => p.Status == "Pending")
                .Select(d => new DoctorRegistrationRequestDto
                {
                    Id = d.Id,
                    UserName = d.UserName, 
                    Specialization = d.Specialization,
                    Number = d.Number,
                    Status = d.Status,
                    MedicalId=d.MedicalId
                })
                .ToListAsync();

            if (!requests.Any())
            {
                return NotFound();
            }

            return Ok(requests);
        }

        

        // PUT: Accept a doctor's registration
        [HttpPut("AcceptDoctor/{doctorId}")]
        public async Task<IActionResult> AcceptDoctor(string doctorId)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId && d.Status == "Pending");

            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found or already processed." });
            }

            doctor.Status = "Accepted";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Doctor request accepted successfully." });
        }

        // PUT: Reject a doctor's registration
        [HttpPut("RejectDoctor/{doctorId}")]
        public async Task<IActionResult> RejectDoctor(string doctorId)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId && d.Status == "Pending");

            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found or already processed." });
            }

            doctor.Status = "Rejected";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Doctor request rejected successfully." });
        }
    }
}
