using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GP.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using GP.DTOs;

using GP.DTOs;
using GP.DTOs.PetMarriageRequest;
using GP.DTOs.Pet;


namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetMarriageRequestController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public PetMarriageRequestController(AuthDbContext context)
        {
            _context = context;
        }

        // GET: api/PetMarriageRequest
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> GetRequests()
        {
            var requests = await _context.PetMarriageRequests
                .Include(r => r.SenderPet)
                    .ThenInclude(p => p.Owner)
                .Include(r => r.SenderPet)
                    .ThenInclude(p => p.Photos) // Include SenderPet photos
                .Include(r => r.ReceiverPet)
                    .ThenInclude(p => p.Owner)
                .Include(r => r.ReceiverPet)
                    .ThenInclude(p => p.Photos) // Include ReceiverPet photos
                .ToListAsync();

            var result = requests.Select(r => MapToGetPetMarriageRequestDto(r)).ToList();

            return Ok(result);
        }

        [HttpGet("MyRequests/{receiverId}")]
        public async Task<IActionResult> GetMyRequests(int receiverId)
        {
            var myRequests = await _context.PetMarriageRequests
                .Include(r => r.SenderPet)
                    .ThenInclude(p => p.Owner)
                .Include(r => r.SenderPet)
                    .ThenInclude(p => p.Photos) // Include SenderPet photos
                .Include(r => r.ReceiverPet)
                    .ThenInclude(p => p.Owner)
                .Include(r => r.ReceiverPet)
                    .ThenInclude(p => p.Photos) // Include ReceiverPet photos
                .Where(r => r.ReceiverPetId == receiverId)
                .ToListAsync();

            var result = myRequests.Select(r => MapToGetPetMarriageRequestDto(r)).ToList();

            if (!result.Any())
            {
                return NotFound("No requests found for this receiver ID.");
            }

            return Ok(result);
        }




        // POST: api/PetMarriageRequest/make
        [HttpPost("MakeMarriageRequest")]
        public async Task<IActionResult> MakeRequest(PetMarriageRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = new PetMarriageRequest
            {
                SenderPetId = requestDto.SenderPetId,
                ReceiverPetId = requestDto.ReceiverPetId,
            };

            _context.Set<PetMarriageRequest>().Add(request);
            await _context.SaveChangesAsync();

            return Ok("Marriage request sent successfully.");
        }

        // PUT: api/PetMarriageRequest/accept/5
        [HttpPut("AcceptMarriageRequest/{id}")]
        public async Task<IActionResult> AcceptRequest(int id)
        {
            var request = await _context.Set<PetMarriageRequest>().FindAsync(id);
            if (request == null)
            {
                return NotFound("Request not found.");
            }

            request.Status = "Accepted";
            await _context.SaveChangesAsync();
            return Ok("Request accepted.");
        }

        // PUT: api/PetMarriageRequest/reject/5
        [HttpPut("RejectMarriageRequest/{id}")]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var request = await _context.Set<PetMarriageRequest>().FindAsync(id);
            if (request == null)
            {
                return NotFound("Request not found.");
            }

            request.Status = "Rejected";
            await _context.SaveChangesAsync();
            return Ok("Request rejected.");
        }






        //private static GetPetMarriageRequestDto MapToGetPetMarriageRequestDto(PetMarriageRequest request)
        private GetPetMarriageRequestDto MapToGetPetMarriageRequestDto(PetMarriageRequest request)
        {
            return new GetPetMarriageRequestDto
            {
                RequestId = request.RequestId,
                SenderPet = new PetResponseDto
                {
                    PetId = request.SenderPet.PetId,
                    Name = request.SenderPet.Name,
                    Age = request.SenderPet.Age,
                    Breed = request.SenderPet.Breed,
                    Gender = request.SenderPet.Gender,
                    HealthStatus = request.SenderPet.HealthStatus,
                    UserId = request.SenderPet.UserId,
                    PhotoUrls = request.SenderPet.Photos.Select(p => p.ImageUrl).ToList(), // Include all photo URLs
                    Owner = new OwnerDto
                    {
                        Id = request.SenderPet.Owner.Id,
                        UserName = request.SenderPet.Owner.UserName
                    }
                },
                ReceiverPet = new PetResponseDto
                {
                    PetId = request.ReceiverPet.PetId,
                    Name = request.ReceiverPet.Name,
                    Age = request.ReceiverPet.Age,
                    Breed = request.ReceiverPet.Breed,
                    Gender = request.ReceiverPet.Gender,
                    HealthStatus = request.ReceiverPet.HealthStatus,
                    UserId = request.ReceiverPet.UserId,
                    PhotoUrls = request.ReceiverPet.Photos.Select(p => p.ImageUrl).ToList(), // Include all photo URLs
                    Owner = new OwnerDto
                    {
                        Id = request.ReceiverPet.Owner.Id,
                        UserName = request.ReceiverPet.Owner.UserName
                    }
                },
                Status = request.Status,
                RequestedAt = request.RequestedAt
            };
        }



    }






}







   






// I added DTOs for creating and retrieving requests! Let me know if you want to tweak anything. 🚀🐾
