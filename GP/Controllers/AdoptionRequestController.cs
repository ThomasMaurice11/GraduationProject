using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GP.DTOs.Adoption;
using GP.DTOs;
using System.Security.Claims;
using GP.DTOs.Pet;
using GP.Exceptions;

namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdoptionRequestsController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public AdoptionRequestsController(AuthDbContext context)
        {
            _context = context;
        }

        // GET: api/AdoptionRequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdoptionRequestResponse>>> GetAdoptionRequests()
        {
            var requests = await _context.AdoptionRequests
                .Include(r => r.Owner)
                .Include(r => r.Pet)
                .Include(r => r.Animal)
                .Select(r => new AdoptionRequestResponse
                {
                    AdoptionRequestId = r.AdoptionRequestId,
                    Requester = new UserDto
                    {
                        Id = r.Owner.Id,
                        UserName = r.Owner.UserName,
                        Email = r.Owner.Email,
                        PhoneNumber = r.Owner.PhoneNumber
                    },
                    PetId = r.PetId,
                    AnimalId = r.AnimalId,
                    Status = r.Status,
                    AnotherPet = r.AnotherPet,
                    OwnedAnimalBefore = r.OwnedAnimalBefore,
                    HoursAnimalAlone = r.HoursAnimalAlone,
                    RequestedAt = r.RequestedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        // GET: api/AdoptionRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AdoptionRequestResponse>> GetAdoptionRequest(int id)
        {
            var adoptionRequest = await _context.AdoptionRequests
                .Include(r => r.Owner)
                .Include(r => r.Pet)
                .Include(r => r.Animal)
                .FirstOrDefaultAsync(r => r.AdoptionRequestId == id);

            if (adoptionRequest == null)
                throw new KeyNotFoundException($"Adoption request with ID {id} not found");

            return Ok(new AdoptionRequestResponse
            {
                AdoptionRequestId = adoptionRequest.AdoptionRequestId,
                Requester = new UserDto
                {
                    Id = adoptionRequest.Owner.Id,
                    UserName = adoptionRequest.Owner.UserName,
                    Email = adoptionRequest.Owner.Email,
                     PhoneNumber = adoptionRequest.Owner.PhoneNumber
                },
                PetId = adoptionRequest.PetId,
                AnimalId = adoptionRequest.AnimalId,
                Status = adoptionRequest.Status,
                AnotherPet = adoptionRequest.AnotherPet,
                OwnedAnimalBefore = adoptionRequest.OwnedAnimalBefore,
                HoursAnimalAlone = adoptionRequest.HoursAnimalAlone,
                RequestedAt = adoptionRequest.RequestedAt
            });
        }

        [HttpGet("GetMyRequests")]
        public async Task<ActionResult<IEnumerable<AdoptionRequestWithDetailsDto>>> GetMyRequests()
        {
            var userId = User?.FindFirst("id")?.Value
                ?? throw new AppException("User not authenticated", 401);

            var requests = await _context.AdoptionRequests
                .Where(r => r.UserId == userId)
                .Include(r => r.Owner)
                .Include(r => r.Pet)
                    .ThenInclude(p => p.Photos)
                .Include(r => r.Animal)
                    .ThenInclude(a => a.Photos)
                .Select(r => new AdoptionRequestWithDetailsDto
                {
                    AdoptionRequestId = r.AdoptionRequestId,
                    UserInfo = new UserDto
                    {
                        Id = r.Owner.Id,
                        UserName = r.Owner.UserName,
                        Email = r.Owner.Email
                    },
                    PetInfo = r.PetId.HasValue ? new PetResponseDto
                    {
                        PetId = r.Pet.PetId,
                        Name = r.Pet.Name,
                        Breed = r.Pet.Breed,
                        Age = r.Pet.Age,
                        Gender = r.Pet.Gender,
                        HealthIssues = r.Pet.HealthIssues,
                        UserId = r.Pet.UserId,
                        PhotoUrls = r.Pet.Photos.Select(i => i.ImageUrl).ToList(),
                        Owner = new OwnerDto
                        {
                            Id = r.Pet.Owner.Id,
                            UserName = r.Pet.Owner.UserName,
                            Email = r.Pet.Owner.Email,
                            PhoneNumber = r.Pet.Owner.PhoneNumber

                        }
                    } : null,
                    AnimalInfo = r.AnimalId.HasValue ? new AnimalResponseDto
                    {
                        AnimalId = r.Animal.AnimalId,
                        Description = r.Animal.Description,
                        PhotoUrls = r.Animal.Photos.Select(i => i.ImageUrl).ToList(),
                        Age = r.Animal.Age,
                        Gender = r.Animal.Gender,
                        HealthIssues = r.Animal.HealthIssues
                    } : null,
                    Status = r.Status,
                    RequestDetails = new RequestDetailsDto
                    {
                        AnotherPet = r.AnotherPet,
                        OwnedAnimalBefore = r.OwnedAnimalBefore,
                        HoursAnimalAlone = r.HoursAnimalAlone
                    },
                    RequestedAt = r.RequestedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPost]
        public async Task<ActionResult<AdoptionRequestResponse>> CreateAdoptionRequest(AdoptionRequestDto createDto)
        {
            var userId = User?.FindFirst("id")?.Value
                ?? throw new AppException("User not authenticated", 401);

            if (createDto.PetId.HasValue && createDto.AnimalId.HasValue)
                throw new AppException("Cannot request both pet and animal simultaneously", 400);

            if (!createDto.PetId.HasValue && !createDto.AnimalId.HasValue)
                throw new AppException("Either PetId or AnimalId must be provided", 400);

            if (createDto.PetId.HasValue && !await _context.Pets.AnyAsync(p => p.PetId == createDto.PetId))
                throw new KeyNotFoundException($"Pet with ID {createDto.PetId} not found");

            if (createDto.AnimalId.HasValue && !await _context.Animals.AnyAsync(a => a.AnimalId == createDto.AnimalId))
                throw new KeyNotFoundException($"Animal with ID {createDto.AnimalId} not found");

            var adoptionRequest = new AdoptionRequest
            {
                UserId = userId,
                PetId = createDto.PetId,
                AnimalId = createDto.AnimalId,
                Status = "Pending",
                AnotherPet = createDto.AnotherPet,
                OwnedAnimalBefore = createDto.OwnedAnimalBefore,
                HoursAnimalAlone = createDto.HoursAnimalAlone,
                RequestedAt = DateTime.UtcNow
            };

            _context.AdoptionRequests.Add(adoptionRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetAdoptionRequest),
                new { id = adoptionRequest.AdoptionRequestId },
                new AdoptionRequestResponse
                {
                    AdoptionRequestId = adoptionRequest.AdoptionRequestId,
                    PetId = adoptionRequest.PetId,
                    AnimalId = adoptionRequest.AnimalId,
                    Status = adoptionRequest.Status,
                    AnotherPet = adoptionRequest.AnotherPet,
                    OwnedAnimalBefore = adoptionRequest.OwnedAnimalBefore,
                    HoursAnimalAlone = adoptionRequest.HoursAnimalAlone,
                    RequestedAt = adoptionRequest.RequestedAt
                });
        }

        [HttpGet("byAnimals/{animalId}")]
        public async Task<ActionResult<IEnumerable<AdoptionRequestResponse>>> GetRequestsByAnimals(int animalId)
        {
            if (!await _context.Animals.AnyAsync(a => a.AnimalId == animalId))
                throw new KeyNotFoundException($"Animal with ID {animalId} not found");

            var requests = await _context.AdoptionRequests
                .Where(r => r.AnimalId == animalId)
                .Select(r => new AdoptionRequestResponse
                {
                    AdoptionRequestId = r.AdoptionRequestId,
                    Status = r.Status,
                    AnimalId = r.AnimalId,
                    AnotherPet = r.AnotherPet,
                    OwnedAnimalBefore = r.OwnedAnimalBefore,
                    HoursAnimalAlone = r.HoursAnimalAlone,
                    RequestedAt = r.RequestedAt,
                    Requester = new UserDto
                    {
                        Id = r.Owner.Id,
                        UserName = r.Owner.UserName,
                        Email = r.Owner.Email,
                        PhoneNumber = r.Owner.PhoneNumber
                    }
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("ByPet/{petId}")]
        public async Task<ActionResult<IEnumerable<AdoptionRequestResponse>>> GetRequestsByPet(int petId)
        {
            if (!await _context.Pets.AnyAsync(p => p.PetId == petId))
                throw new KeyNotFoundException($"Pet with ID {petId} not found");

            var requests = await _context.AdoptionRequests
                .Where(r => r.PetId == petId)
                .Include(r => r.Owner)
                .Select(r => new AdoptionRequestResponse
                {
                    AdoptionRequestId = r.AdoptionRequestId,
                    Status = r.Status,
                    PetId = r.PetId,
                    AnotherPet = r.AnotherPet,
                    OwnedAnimalBefore = r.OwnedAnimalBefore,
                    HoursAnimalAlone = r.HoursAnimalAlone,
                    RequestedAt = r.RequestedAt,
                    Requester = new UserDto
                    {
                        Id = r.Owner.Id,
                        UserName = r.Owner.UserName,
                        Email = r.Owner.Email,
                        PhoneNumber = r.Owner.PhoneNumber
                    }
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdoptionRequest(int id)
        {
            var adoptionRequest = await _context.AdoptionRequests.FindAsync(id)
                ?? throw new KeyNotFoundException($"Adoption request with ID {id} not found");

            _context.AdoptionRequests.Remove(adoptionRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("PendingRequests")]
        public async Task<ActionResult<IEnumerable<AdoptionRequestResponse>>> GetPendingAdoptionRequests()
        {
            var requests = await _context.AdoptionRequests
                .Where(r => r.AnimalId != null && r.Status == "Pending")
                .Include(r => r.Owner)
                .Include(r => r.Animal)
                    .ThenInclude(a => a.Photos)
                .Select(r => new AdoptionRequestResponse
                {
                    AdoptionRequestId = r.AdoptionRequestId,
                    Requester = new UserDto
                    {
                        Id = r.Owner.Id,
                        UserName = r.Owner.UserName,
                        Email = r.Owner.Email,
                        PhoneNumber = r.Owner.PhoneNumber
                    },
                    AnimalId = r.AnimalId,
                    AnimalInfo = r.AnimalId.HasValue ? new AnimalResponseDto
                    {
                        AnimalId = r.Animal.AnimalId,
                        Description = r.Animal.Description,
                        PhotoUrls = r.Animal.Photos.Select(i => i.ImageUrl).ToList(),
                        Age = r.Animal.Age,
                        Gender = r.Animal.Gender,
                        HealthIssues = r.Animal.HealthIssues
                    } : null,
                    Status = r.Status,
                    AnotherPet = r.AnotherPet,
                    OwnedAnimalBefore = r.OwnedAnimalBefore,
                    HoursAnimalAlone = r.HoursAnimalAlone,
                    RequestedAt = r.RequestedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("RejectedRequests")]
        public async Task<ActionResult<IEnumerable<AdoptionRequestResponse>>> GetRejectedAdoptionRequests()
        {
            var requests = await _context.AdoptionRequests
                .Where(r => r.AnimalId != null && r.Status == "Rejected")
                .Include(r => r.Owner)
                .Include(r => r.Animal)
                    .ThenInclude(a => a.Photos)
                .Select(r => new AdoptionRequestResponse
                {
                    AdoptionRequestId = r.AdoptionRequestId,
                    Requester = new UserDto
                    {
                        Id = r.Owner.Id,
                        UserName = r.Owner.UserName,
                        Email = r.Owner.Email,
                        PhoneNumber = r.Owner.PhoneNumber
                    },
                    AnimalId = r.AnimalId,
                    AnimalInfo = r.AnimalId.HasValue ? new AnimalResponseDto
                    {
                        AnimalId = r.Animal.AnimalId,
                        Description = r.Animal.Description,
                        PhotoUrls = r.Animal.Photos.Select(i => i.ImageUrl).ToList(),
                        Age = r.Animal.Age,
                        Gender = r.Animal.Gender,
                        HealthIssues = r.Animal.HealthIssues
                    } : null,
                    Status = r.Status,
                    AnotherPet = r.AnotherPet,
                    OwnedAnimalBefore = r.OwnedAnimalBefore,
                    HoursAnimalAlone = r.HoursAnimalAlone,
                    RequestedAt = r.RequestedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("AcceptedRequests")]
        public async Task<ActionResult<IEnumerable<AdoptionRequestResponse>>> GetAcceptedAdoptionRequests()
        {
            var requests = await _context.AdoptionRequests
                .Where(r => r.AnimalId != null && r.Status == "Accepted")
                .Include(r => r.Owner)
                .Include(r => r.Animal)
                    .ThenInclude(a => a.Photos)
                .Select(r => new AdoptionRequestResponse
                {
                    AdoptionRequestId = r.AdoptionRequestId,
                    Requester = new UserDto
                    {
                        Id = r.Owner.Id,
                        UserName = r.Owner.UserName,
                        Email = r.Owner.Email,
                        PhoneNumber = r.Owner.PhoneNumber
                    },
                    AnimalId = r.AnimalId,
                    AnimalInfo = r.AnimalId.HasValue ? new AnimalResponseDto
                    {
                        AnimalId = r.Animal.AnimalId,
                        Description = r.Animal.Description,
                        PhotoUrls = r.Animal.Photos.Select(i => i.ImageUrl).ToList(),
                        Age = r.Animal.Age,
                        Gender = r.Animal.Gender,
                        HealthIssues = r.Animal.HealthIssues
                    } : null,
                    Status = r.Status,
                    AnotherPet = r.AnotherPet,
                    OwnedAnimalBefore = r.OwnedAnimalBefore,
                    HoursAnimalAlone = r.HoursAnimalAlone,
                    RequestedAt = r.RequestedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPatch("Accept/{requestId}")]
        public async Task<IActionResult> AcceptAdoptionRequest(int requestId)
        {
            var adoptionRequest = await _context.AdoptionRequests.FindAsync(requestId)
                ?? throw new KeyNotFoundException($"Adoption request with ID {requestId} not found");

            if (adoptionRequest.Status != "Pending")
                throw new AppException("Only pending requests can be accepted", 400);

            adoptionRequest.Status = "Accepted";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("Reject/{requestId}")]
        public async Task<IActionResult> RejectAdoptionRequest(int requestId)
        {
            var adoptionRequest = await _context.AdoptionRequests.FindAsync(requestId)
                ?? throw new KeyNotFoundException($"Adoption request with ID {requestId} not found");

            if (adoptionRequest.Status != "Pending")
                throw new AppException("Only pending requests can be rejected", 400);

            adoptionRequest.Status = "Rejected";
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}