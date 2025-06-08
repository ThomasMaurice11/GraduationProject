using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GP.Models;
using GP.DTOs.Clinic;
using System.Linq;
using System.Threading.Tasks;
using GP.Services;
using Microsoft.AspNetCore.Authorization;
using GP.DTOs.Pet;
using GP.DTOs;
using GP.Exceptions;

namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly JwtTokenService _jwtTokenService;

        public ClinicController(AuthDbContext context, JwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
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

        // Updated GetClinics method to only return Accepted clinics
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetClinicDto>>> GetClinics()
        {
            var clinics = await _context.Clinics
                .Include(c => c.Doctor)
                .Where(c => c.Status == "Accepted")
                .ToListAsync();

            return Ok(clinics.Select(MapToGetClinicDto));
        }

        // Updated GetClinic method to only return if Accepted
        [HttpGet("{id}")]
        public async Task<ActionResult<GetClinicDto>> GetClinic(int id)
        {
            var clinic = await _context.Clinics
                .Include(c => c.Doctor)
                .FirstOrDefaultAsync(c => c.ClinicId == id && c.Status == "Accepted");

            if (clinic == null)
                throw new AppException($"Clinic with ID {id} not found or not accepted", 404, "Not Found");

            return Ok(MapToGetClinicDto(clinic));
        }

        [HttpGet("MyClinics")]
        public async Task<ActionResult<IEnumerable<GetClinicDto>>> GetMyClinics()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var doctorId = _jwtTokenService.GetUserIdFromToken(token);

            var clinics = await _context.Clinics
                .Include(c => c.Doctor)
                .Where(c => c.DoctorId == doctorId)
                .ToListAsync();

            return Ok(clinics.Select(MapToGetClinicDto));
        }

        [HttpPost]
        public async Task<ActionResult<GetClinicDto>> AddClinic(AddClinicDto addClinic)
        {
            if (addClinic == null)
                throw new AppException("Clinic data must be provided", 400, "Bad Request");

            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var doctorId = _jwtTokenService.GetUserIdFromToken(token);

            var clinic = new Clinic
            {
                DoctorId = doctorId,
                Name = addClinic.Name,
                Address = addClinic.Address,
                LocationUrl = addClinic.LocationUrl,
                Details = addClinic.Details,
                Number = addClinic.Number,
                CLinicEmail = addClinic.CLinicEmail,
                Status = "Pending"
            };

            _context.Clinics.Add(clinic);
            await _context.SaveChangesAsync();

            // Retrieve the clinic with doctor info after saving
            var createdClinic = await _context.Clinics
                .Include(c => c.Doctor)
                .FirstOrDefaultAsync(c => c.ClinicId == clinic.ClinicId);

            if (createdClinic == null)
                throw new AppException($"Clinic with ID {clinic.ClinicId} not found after creation", 500, "Internal Server Error");

            return CreatedAtAction(nameof(GetClinic), new { id = createdClinic.ClinicId }, MapToGetClinicDto(createdClinic));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutClinic(int id, UpdateClinicDto updateClinicDto)
        {
            if (updateClinicDto == null)
                throw new AppException("Clinic update data must be provided", 400, "Bad Request");

            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null)
                throw new AppException($"Clinic with ID {id} not found", 404, "Not Found");

            clinic.Name = updateClinicDto.Name;
            clinic.Address = updateClinicDto.Address;
            clinic.LocationUrl = updateClinicDto.LocationUrl;
            clinic.Details = updateClinicDto.Details;
            clinic.Number = updateClinicDto.Number;
            clinic.CLinicEmail = updateClinicDto.CLinicEmail;

            _context.Entry(clinic).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClinicExists(id))
                    throw new AppException($"Clinic with ID {id} no longer exists", 404, "Not Found");
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClinic(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null)
                throw new AppException($"Clinic with ID {id} not found", 404, "Not Found");

            _context.Clinics.Remove(clinic);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    
        private bool ClinicExists(int id)
        {
            return _context.Clinics.Any(e => e.ClinicId == id);
        }
    }
}