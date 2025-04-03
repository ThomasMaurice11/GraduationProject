using Azure.Core;
using GP.DTOs.Pet;
using GP.DTOs;
using GP.Models;
using GP.Services;
using GP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Office.Word;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PetController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly JwtTokenService _jwtTokenService;

    public PetController(AuthDbContext context, IWebHostEnvironment env, JwtTokenService jwtTokenService)
    {
        _context = context;
        _env = env;
        _jwtTokenService = jwtTokenService;
    }
    [HttpPost]
    public async Task<IActionResult> CreatePet([FromForm] PetCreateDto petDto)
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var userId = _jwtTokenService.GetUserIdFromToken(token);
        if (userId == null)
        {
            return Unauthorized("User ID not found in token.");
        }

        if (petDto.Photos == null || petDto.Photos.Count == 0)
            return BadRequest("No files uploaded.");

        // 1. First create and save the pet to get its ID
        var pet = new Pet
        {
            Name = petDto.Name,
            Age = petDto.Age,
            Breed = petDto.Breed,
            Gender = petDto.Gender,
            HealthStatus = petDto.HealthStatus,
            UserId = userId,
        };

        _context.Pets.Add(pet);
        await _context.SaveChangesAsync(); // This generates the PetId

        // 2. Now add photos with the correct PetId
        foreach (var photo in petDto.Photos)
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
                PetId = pet.PetId // Now this has a valid value
            });
        }

        // 3. Save the photos
        await _context.SaveChangesAsync();

        return Ok(MapToPetResponseDto(pet));
    }

    // GET: api/Pet/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPet(int id)
    {
        var pet = await _context.Pets
            .Include(p => p.Owner)
            .Include(p => p.Photos) // Include photos
            .FirstOrDefaultAsync(p => p.PetId == id);

        if (pet == null)
        {
            return NotFound();
        }

        return Ok(MapToPetResponseDto(pet));
    }

    // GET: api/Pet/GetAllPets
    [HttpGet("GetAllPets")]
    public async Task<IActionResult> GetAllPets()
    {
        var pets = await _context.Pets
            .Include(p => p.Owner)
            .Include(p => p.Photos) // Include photos
            .ToListAsync();

        return Ok(pets.Select(MapToPetResponseDto));
    }

    // GET: api/Pet/GetMyPets
    [HttpGet("GetMyPets")]
    public async Task<IActionResult> GetMyPets()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var userId = _jwtTokenService.GetUserIdFromToken(token);

        var pets = await _context.Pets
            .Include(p => p.Owner)
            .Include(p => p.Photos) // Include photos
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return Ok(pets.Select(MapToPetResponseDto));
    }

    // PUT: api/Pet/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePet(int id, [FromForm] PetUpdateDto petDto)
    {
        var pet = await _context.Pets
            .Include(p => p.Owner)
            .Include(p => p.Photos) // Include photos
            .FirstOrDefaultAsync(p => p.PetId == id);

        if (pet == null)
        {
            return NotFound();
        }

        pet.Name = petDto.Name;
        pet.Age = petDto.Age;
        pet.Breed = petDto.Breed;
        pet.Gender = petDto.Gender;
        pet.HealthStatus = petDto.HealthStatus;

        // Update photos if new photos are provided
        if (petDto.Photos != null && petDto.Photos.Count > 0)
        {
            // Remove existing photos
            _context.Photos.RemoveRange(pet.Photos);

            foreach (var photo in petDto.Photos)
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
                    PetId = pet.PetId // Associate the photo with the pet
                });
               
            }
        }

        _context.Pets.Update(pet);
        await _context.SaveChangesAsync();

        return Ok(MapToPetResponseDto(pet));
    }

    // DELETE: api/Pet/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePet(int id)
    {
        var pet = await _context.Pets
            .Include(p => p.Photos) // Include photos
            .FirstOrDefaultAsync(p => p.PetId == id);

        if (pet == null)
        {
            return NotFound();
        }

        // Delete associated photos
        _context.Photos.RemoveRange(pet.Photos);
        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private PetResponseDto MapToPetResponseDto(Pet pet)
    {
        return new PetResponseDto
        {
            PetId = pet.PetId,
            Name = pet.Name,
            Age = pet.Age,
            Breed = pet.Breed,
            Gender = pet.Gender,
            HealthStatus = pet.HealthStatus,
            UserId = pet.UserId,
            PhotoUrls = pet.Photos.Select(p => p.ImageUrl).ToList(), // Include photo URLs
            Owner = pet.Owner != null ? new OwnerDto
            {
                Id = pet.Owner.Id,
                UserName = pet.Owner.UserName,
                Email = pet.Owner.Email
            } : null
        };
    }
}