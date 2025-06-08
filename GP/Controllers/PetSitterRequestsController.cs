using GP.DTOs.PetSitterRequest;
using GP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PetSitterRequestsController : ControllerBase
    {
  
       
            private readonly AuthDbContext _context;

            public PetSitterRequestsController(AuthDbContext context)
            {
                _context = context;
            }

            // GET: api/PetSitterRequests
            [HttpGet]
            public async Task<ActionResult<IEnumerable<PetSitterRequest>>> GetPetSitterRequests()
            {
                return await _context.PetSitterRequests
                    .Include(r => r.Owner)
                    .ToListAsync();
            }

            // GET: api/PetSitterRequests/5
            [HttpGet("{id}")]
            public async Task<ActionResult<PetSitterRequest>> GetPetSitterRequest(int id)
            {
                var petSitterRequest = await _context.PetSitterRequests
                    .Include(r => r.Owner)
                    .FirstOrDefaultAsync(r => r.PetSitterRequestId == id);

                if (petSitterRequest == null)
                {
                    return NotFound();
                }

                return petSitterRequest;
            }

            // GET: api/PetSitterRequests/user/{userId}
            [HttpGet("user/{userId}")]
            public async Task<ActionResult<IEnumerable<PetSitterRequest>>> GetPetSitterRequestsByUser(string userId)
            {
                return await _context.PetSitterRequests
                    .Where(r => r.UserId == userId)
                    .Include(r => r.Owner)
                    .ToListAsync();
            }

            // POST: api/PetSitterRequests
            [HttpPost]
            public async Task<ActionResult<PetSitterRequest>> PostPetSitterRequest(PetSitterRequestDto requestDto)
            {
            var userId = User?.FindFirst("id")?.Value;
            if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate dates
                if (requestDto.StartDate >= requestDto.EndtDate)
                {
                    return BadRequest("End date must be after start date.");
                }

                var petSitterRequest = new PetSitterRequest
                {
                    UserId = userId,
                    Number = requestDto.Number,
                    Status = "Pending", // Default status
                    Breed = requestDto.Breed,
                    Age = requestDto.Age,
                    StartDate = requestDto.StartDate,
                    EndtDate = requestDto.EndtDate,
                    HealthIssues = requestDto.HealthIssues,
                    Notes = requestDto.Notes,
                    RequestedAt = DateTime.UtcNow
                };

                _context.PetSitterRequests.Add(petSitterRequest);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPetSitterRequest),
                    new { id = petSitterRequest.PetSitterRequestId },
                    petSitterRequest);
            }

            // PUT: api/PetSitterRequests/5
            [HttpPut("{id}")]
            public async Task<IActionResult> PutPetSitterRequest(int id, PetSitterRequestDto requestDto)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var petSitterRequest = await _context.PetSitterRequests.FindAsync(id);
                if (petSitterRequest == null)
                {
                    return NotFound();
                }

                // Validate dates
                if (requestDto.StartDate >= requestDto.EndtDate)
                {
                    return BadRequest("End date must be after start date.");
                }

                petSitterRequest.Number = requestDto.Number;
                petSitterRequest.StartDate = requestDto.StartDate;
                petSitterRequest.EndtDate = requestDto.EndtDate;
                petSitterRequest.HealthIssues = requestDto.HealthIssues;
                petSitterRequest.Notes = requestDto.Notes;
                petSitterRequest.Age = requestDto.Age;
                petSitterRequest.Breed = requestDto.Breed;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PetSitterRequestExists(id))
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

            //// PATCH: api/PetSitterRequests/5/status
            //[HttpPatch("{id}/status")]
            //public async Task<IActionResult> UpdateRequestStatus(int id, [FromBody] UpdateStatusDto statusDto)
            //{
            //    var validStatuses = new[] { "Pending", "Accepted", "Rejected" };
            //    if (!validStatuses.Contains(statusDto.Status))
            //    {
            //        return BadRequest("Invalid status value.");
            //    }

            //    var petSitterRequest = await _context.PetSitterRequests.FindAsync(id);
            //    if (petSitterRequest == null)
            //    {
            //        return NotFound();
            //    }

            //    petSitterRequest.Status = statusDto.Status;
            //    await _context.SaveChangesAsync();

            //    return NoContent();
            //}

            // DELETE: api/PetSitterRequests/5
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeletePetSitterRequest(int id)
            {
                var petSitterRequest = await _context.PetSitterRequests.FindAsync(id);
                if (petSitterRequest == null)
                {
                    return NotFound();
                }

                _context.PetSitterRequests.Remove(petSitterRequest);
                await _context.SaveChangesAsync();

                return NoContent();
            }

            private bool PetSitterRequestExists(int id)
            {
                return _context.PetSitterRequests.Any(e => e.PetSitterRequestId == id);
            }
        }
    }
