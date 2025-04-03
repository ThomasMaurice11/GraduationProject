
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
                    // Map Doctor properties to DoctorDataDto
                    Id = clinic.Doctor.Id,
                    UserName = clinic.Doctor.UserName,
                    Email = clinic.Doctor.Email,
                    Specialization = clinic.Doctor.Specialization,
                    Number = clinic.Doctor.Number,
                    MedicalId=clinic.Doctor.MedicalId

                },
                Name = clinic.Name,
                Address = clinic.Address,
                Details = clinic.Details,
                Number = clinic.Number,
                CLinicEmail = clinic.CLinicEmail
                
            };
        }



        // GET: api/Clinic
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetClinicDto>>> GetClinics()
        {
            var clinics = await _context.Clinics
                .Include(c => c.Doctor) // Include related Doctor data
                .ToListAsync();

            return Ok(clinics.Select(c => MapToGetClinicDto(c)));
        }

        // GET: api/Clinic/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<GetClinicDto>>> GetClinic(int id)
        {
            var clinic = await _context.Clinics
                .Include(c => c.Doctor) // Include related Doctor data
                .Where(c => c.ClinicId == id)
                .FirstOrDefaultAsync();

            if (clinic == null)
            {
                return NotFound();
            }
            // Map the single clinic to GetClinicDto
            var clinicResult = MapToGetClinicDto(clinic);
            return Ok(clinicResult);
        }

        // GET: api/Clinic/5
        [HttpGet("MyCLinics")]
        public async Task<ActionResult<IEnumerable<GetClinicDto>>> GetMyClinics()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var doctorId = _jwtTokenService.GetUserIdFromToken(token);


            var clinics = await _context.Clinics
                .Include(c => c.Doctor) // Include related Doctor data
                .Where(c => c.DoctorId == doctorId)
                .ToListAsync();




            // If no clinics are found, return an empty list (no need for NotFound)
            if (!clinics.Any())
            {
                return Ok(new List<GetClinicDto>()); // Return an empty list
            }

            // Map clinics to GetClinicDto and return the result
            return Ok(clinics.Select(c => MapToGetClinicDto(c)));
        }


        // POST: api/Clinic
        [HttpPost]
            public async Task<ActionResult> AddClinic(AddClinicDto addClinic)
            {


                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var doctorId = _jwtTokenService.GetUserIdFromToken(token);

                var clinic = new Clinic
                {
                    DoctorId = doctorId, 
                    Name = addClinic.Name,
                    Address = addClinic.Address,
                    Details = addClinic.Details,
                    Number = addClinic.Number,
                    CLinicEmail = addClinic.CLinicEmail,
                    Status = "Pending" // Default status
                };

                _context.Clinics.Add(clinic);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetClinic", new { id = clinic.ClinicId }, clinic);
            }

        // PUT: api/Clinic/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClinic(int id, UpdateClinicDto updateClinicDto)
        {
            // Find the clinic by ID
            var clinic = await _context.Clinics.FindAsync(id);

            if (clinic == null)
            {
                return NotFound();
            }

            // Update the clinic with the DTO values
            clinic.Name = updateClinicDto.Name;
            clinic.Address = updateClinicDto.Address;
            clinic.Details = updateClinicDto.Details;
            clinic.Number = updateClinicDto.Number;
            clinic.CLinicEmail = updateClinicDto.CLinicEmail;

            // Mark the entity as modified
            _context.Entry(clinic).State = EntityState.Modified;

            try
            {
                // Save changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClinicExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Clinic/5
        [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteClinic(int id)
            {
                var clinic = await _context.Clinics.FindAsync(id);
                if (clinic == null)
                {
                    return NotFound();
                }

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
