using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GP.DTOs;
using GP.Models;
using GP.DTOs.Admin;
using GP.DTOs.Clinic;
using GP.Exceptions;
using Microsoft.AspNetCore.Authorization;

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
        private GetClinicDto MapToGetClinicDto(Clinic clinic)
        {
            return new GetClinicDto
            {
                ClinicId = clinic.ClinicId,
                DoctorId = clinic.DoctorId,
                Doctor = new DoctorDataDto
                {
                    Id = clinic.Doctor.Id,
                    UserName = clinic.Doctor.UserName,
                    Email = clinic.Doctor.Email,
                    Specialization = clinic.Doctor.Specialization,
                    Number = clinic.Doctor.Number,
                    MedicalId = clinic.Doctor.MedicalId
                },
                Name = clinic.Name,
                Address = clinic.Address,
                LocationUrl = clinic.LocationUrl,
                Details = clinic.Details,
                Number = clinic.Number,
                CLinicEmail = clinic.CLinicEmail,
                Status = clinic.Status
            };
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


        // New endpoint to get pending clinics (admin only)
        [HttpGet("PendingCLinics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<GetClinicDto>>> GetPendingClinics()
        {
            var clinics = await _context.Clinics
                .Include(c => c.Doctor)
                .Where(c => c.Status == "Pending")
                .ToListAsync();

            return Ok(clinics.Select(MapToGetClinicDto));
        }

        // New endpoint to accept a clinic (admin only)
        [HttpPut("{id}/AcceptClinics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AcceptClinic(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null)
                throw new AppException($"Clinic with ID {id} not found", 404, "Not Found");

            clinic.Status = "Accepted";
            _context.Entry(clinic).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // New endpoint to reject a clinic (admin only)
        [HttpPut("{id}/RejectClinics")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectClinic(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null)
                throw new AppException($"Clinic with ID {id} not found", 404, "Not Found");

            clinic.Status = "Rejected";
            _context.Entry(clinic).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
