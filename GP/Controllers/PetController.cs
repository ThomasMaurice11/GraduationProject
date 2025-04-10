using Azure.Core;
using GP.DTOs.Pet;
using GP.DTOs;
using GP.Models;
using GP.Services;
using GP.Exceptions;
using GP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PetController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly JwtTokenService _jwtTokenService;
    private readonly ILogger<PetController> _logger;

    public PetController(
        AuthDbContext context,
        IWebHostEnvironment env,
        JwtTokenService jwtTokenService,
        ILogger<PetController> logger)
    {
        _context = context;
        _env = env;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePet([FromForm] PetCreateDto petDto)
    {
        try
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var userId = _jwtTokenService.GetUserIdFromToken(token);

            if (userId == null)
            {
                throw new AppException("User ID not found in token", StatusCodes.Status401Unauthorized, "Unauthorized");
            }

            if (petDto.Photos == null || petDto.Photos.Count == 0)
            {
                throw new AppException("At least one photo is required", StatusCodes.Status400BadRequest, "Validation Error");
            }

            var pet = new Pet
            {
                Title = petDto.Title,
                Name = petDto.Name,
                Age = petDto.Age,
                Breed = petDto.Breed,
                Gender = petDto.Gender,
                HealthIssues = petDto.HealthIssues,
                Description = petDto.Description,
                UserId = userId,
            };

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            foreach (var photo in petDto.Photos)
            {
                try
                {
                    using var memoryStream = new MemoryStream();
                    await photo.CopyToAsync(memoryStream);
                    var photoBytes = memoryStream.ToArray();

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                    var filePath = Path.Combine(_env.WebRootPath, "images", fileName);
                    await System.IO.File.WriteAllBytesAsync(filePath, photoBytes);

                    pet.Photos.Add(new Photo
                    {
                        ImageData = photoBytes,
                        ImageUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}",
                        PetId = pet.PetId
                    });
                }
                catch (IOException ioEx)
                {
                    _logger.LogError(ioEx, "Error saving photo for pet {PetId}", pet.PetId);
                    throw new AppException("Error saving photo", StatusCodes.Status500InternalServerError, "File System Error");
                }
            }

            await _context.SaveChangesAsync();
            return Ok(MapToPetResponseDto(pet));
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while creating pet");
            throw new AppException("Database error while saving pet", StatusCodes.Status500InternalServerError, "Database Error");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPet(int id)
    {
        var pet = await _context.Pets
            .Include(p => p.Owner)
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.PetId == id);

        if (pet == null)
        {
            throw new AppException($"Pet with ID {id} not found", StatusCodes.Status404NotFound, "Not Found");
        }

        return Ok(MapToPetResponseDto(pet));
    }

    [HttpGet("GetAllPets")]
    public async Task<IActionResult> GetAllPets()
    {
        try
        {
            var pets = await _context.Pets
                .Include(p => p.Owner)
                .Include(p => p.Photos)
                .ToListAsync();

            return Ok(pets.Select(MapToPetResponseDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all pets");
            throw new AppException("Error retrieving pets", StatusCodes.Status500InternalServerError, "Database Error");
        }
    }

    [HttpGet("GetMyPets")]
    public async Task<IActionResult> GetMyPets()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var userId = _jwtTokenService.GetUserIdFromToken(token);

        if (userId == null)
        {
            throw new AppException("User ID not found in token", StatusCodes.Status401Unauthorized, "Unauthorized");
        }

        var pets = await _context.Pets
            .Include(p => p.Owner)
            .Include(p => p.Photos)
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return Ok(pets.Select(MapToPetResponseDto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePet(int id, [FromForm] PetUpdateDto petDto)
    {
        try
        {
            var pet = await _context.Pets
                .Include(p => p.Owner)
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.PetId == id);

            if (pet == null)
            {
                throw new AppException($"Pet with ID {id} not found", StatusCodes.Status404NotFound, "Not Found");
            }

            pet.Title = petDto.Title;
            pet.Name = petDto.Name;
            pet.Age = petDto.Age;
            pet.Breed = petDto.Breed;
            pet.Gender = petDto.Gender;
            pet.HealthIssues = petDto.HealthIssues;
            pet.Description = petDto.Description;

            if (petDto.Photos != null && petDto.Photos.Count > 0)
            {
                _context.Photos.RemoveRange(pet.Photos);

                foreach (var photo in petDto.Photos)
                {
                    try
                    {
                        using var memoryStream = new MemoryStream();
                        await photo.CopyToAsync(memoryStream);
                        var photoBytes = memoryStream.ToArray();

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                        var filePath = Path.Combine(_env.WebRootPath, "images", fileName);
                        await System.IO.File.WriteAllBytesAsync(filePath, photoBytes);

                        pet.Photos.Add(new Photo
                        {
                            ImageData = photoBytes,
                            ImageUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}",
                            PetId = pet.PetId
                        });
                    }
                    catch (IOException ioEx)
                    {
                        _logger.LogError(ioEx, "Error updating photo for pet {PetId}", pet.PetId);
                        throw new AppException("Error updating photo", StatusCodes.Status500InternalServerError, "File System Error");
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok(MapToPetResponseDto(pet));
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while updating pet {PetId}", id);
            throw new AppException("Database error while updating pet", StatusCodes.Status500InternalServerError, "Database Error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePet(int id)
    {
        try
        {
            var pet = await _context.Pets
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.PetId == id);

            if (pet == null)
            {
                throw new AppException($"Pet with ID {id} not found", StatusCodes.Status404NotFound, "Not Found");
            }

            _context.Photos.RemoveRange(pet.Photos);
            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while deleting pet {PetId}", id);
            throw new AppException("Database error while deleting pet", StatusCodes.Status500InternalServerError, "Database Error");
        }
    }

    private PetResponseDto MapToPetResponseDto(Pet pet)
    {
        return new PetResponseDto
        {
            PetId = pet.PetId,
            Title = pet.Title,
            Name = pet.Name,
            Age = pet.Age,
            Breed = pet.Breed,
            Gender = pet.Gender,
            HealthIssues = pet.HealthIssues,
            Description = pet.Description,
            UserId = pet.UserId,
            PhotoUrls = pet.Photos.Select(p => p.ImageUrl).ToList(),
            Owner = pet.Owner != null ? new OwnerDto
            {
                Id = pet.Owner.Id,
                UserName = pet.Owner.UserName,
                Email = pet.Owner.Email
            } : null
        };
    }
}