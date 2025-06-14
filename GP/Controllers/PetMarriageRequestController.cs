﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GP.Models;
using System.Linq;
using System.Threading.Tasks;
using GP.DTOs;
using GP.DTOs.PetMarriageRequest;
using GP.DTOs.Pet;
using GP.Exceptions;

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

        [HttpGet]
        public async Task<IActionResult> GetRequests()
        {
            try
            {
                var requests = await _context.PetMarriageRequests
                    .Include(r => r.SenderPet)
                        .ThenInclude(p => p.Owner)
                    .Include(r => r.SenderPet)
                        .ThenInclude(p => p.Photos)
                    .Include(r => r.ReceiverPet)
                        .ThenInclude(p => p.Owner)
                    .Include(r => r.ReceiverPet)
                        .ThenInclude(p => p.Photos)
                    .ToListAsync();

                var result = requests.Select(r => MapToGetPetMarriageRequestDto(r)).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                throw new AppException($"Failed to fetch pet marriage requests: {ex.Message}");
            }
        }

        [HttpGet("MyRequests/{receiverId}")]
        public async Task<IActionResult> GetMyRequests(int receiverId)
        {
            try
            {
                var myRequests = await _context.PetMarriageRequests
                    .Include(r => r.SenderPet)
                        .ThenInclude(p => p.Owner)
                    .Include(r => r.SenderPet)
                        .ThenInclude(p => p.Photos)
                    .Include(r => r.ReceiverPet)
                        .ThenInclude(p => p.Owner)
                    .Include(r => r.ReceiverPet)
                        .ThenInclude(p => p.Photos)
                    .Where(r => r.ReceiverPetId == receiverId)
                    .ToListAsync();

                var result = myRequests.Select(r => MapToGetPetMarriageRequestDto(r)).ToList();

                if (!result.Any())
                    throw new AppException("No requests found for this receiver ID.", 404, "Not Found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                throw new AppException($"Failed to fetch requests for receiver ID {receiverId}: {ex.Message}");
            }
        }

        [HttpPost("MakeMarriageRequest")]
        public async Task<IActionResult> MakeRequest(PetMarriageRequestDto requestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new AppException("Invalid request data", 400, "Bad Request");

                var senderExists = await _context.Pets.AnyAsync(p => p.PetId == requestDto.SenderPetId);
                var receiverExists = await _context.Pets.AnyAsync(p => p.PetId == requestDto.ReceiverPetId);

                if (!senderExists || !receiverExists)
                    throw new AppException("Sender or Receiver Pet does not exist", 404, "Not Found");

                var request = new PetMarriageRequest
                {
                    SenderPetId = requestDto.SenderPetId,
                    ReceiverPetId = requestDto.ReceiverPetId,
                };

                _context.PetMarriageRequests.Add(request);
                await _context.SaveChangesAsync();

                return Ok("Marriage request sent successfully.");
            }
            catch (Exception ex)
            {
                throw new AppException($"Failed to create marriage request: {ex.Message}");
            }
        }

        [HttpPut("AcceptMarriageRequest/{id}")]
        public async Task<IActionResult> AcceptRequest(int id)
        {
            try
            {
                var request = await _context.PetMarriageRequests.FindAsync(id);
                if (request == null)
                    throw new AppException("Request not found.", 404, "Not Found");

                request.Status = "Accepted";
                await _context.SaveChangesAsync();
                return Ok("Request accepted.");
            }
            catch (Exception ex)
            {
                throw new AppException($"Failed to accept request: {ex.Message}");
            }
        }

        [HttpPut("RejectMarriageRequest/{id}")]
        public async Task<IActionResult> RejectRequest(int id)
        {
            try
            {
                var request = await _context.PetMarriageRequests.FindAsync(id);
                if (request == null)
                    throw new AppException("Request not found.", 404, "Not Found");

                request.Status = "Rejected";
                await _context.SaveChangesAsync();
                return Ok("Request rejected.");
            }
            catch (Exception ex)
            {
                throw new AppException($"Failed to reject request: {ex.Message}");
            }
        }

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
                    HealthIssues = request.SenderPet.HealthIssues,
                    UserId = request.SenderPet.UserId,
                    PhotoUrls = request.SenderPet.Photos.Select(p => p.ImageUrl).ToList(),
                    Owner = new OwnerDto
                    {
                        Id = request.SenderPet.Owner.Id,
                        UserName = request.SenderPet.Owner.UserName,
                        PhoneNumber = request.SenderPet.Owner.PhoneNumber
                    }
                },
                ReceiverPet = new PetResponseDto
                {
                    PetId = request.ReceiverPet.PetId,
                    Name = request.ReceiverPet.Name,
                    Age = request.ReceiverPet.Age,
                    Breed = request.ReceiverPet.Breed,
                    Gender = request.ReceiverPet.Gender,
                    HealthIssues = request.ReceiverPet.HealthIssues,
                    UserId = request.ReceiverPet.UserId,
                    PhotoUrls = request.ReceiverPet.Photos.Select(p => p.ImageUrl).ToList(),
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
