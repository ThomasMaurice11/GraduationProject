using Azure.Core;
using GP.Models;
using GP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class AnimalsController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AnimalsController(AuthDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAnimal([FromForm] AnimalCreateDto animalDto)
    {
        if (animalDto.Photos == null || animalDto.Photos.Count == 0)
            return BadRequest("No files uploaded.");

        var animal = new Animal
        {
            Description = animalDto.Description,
            Age = animalDto.Age,
            Gender = animalDto.Gender,
            HealthIssues = animalDto.HealthIssues,
        };

        // First save the animal to get an ID
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync(); // This generates the AnimalId

        foreach (var photo in animalDto.Photos)
        {
            using var memoryStream = new MemoryStream();
            await photo.CopyToAsync(memoryStream);
            var photoBytes = memoryStream.ToArray();

            var fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
            var filePath = Path.Combine(_env.WebRootPath, "images", fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, photoBytes);

            animal.Photos.Add(new Photo
            {
                ImageData = photoBytes,
                ImageUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}",
                AnimalId = animal.AnimalId, // Set AnimalId
                PetId = null // Explicitly set PetId to null
            });
        }

        await _context.SaveChangesAsync();
        return Ok(MapToAnimalResponseDto(animal));
    }

    // GET: api/Animals
    [HttpGet]
    public async Task<IActionResult> GetAllAnimals()
    {
        var animals = await _context.Animals
            .Include(a => a.Photos) // Include photos
            .Select(a => MapToAnimalResponseDto(a))
            .ToListAsync();

        return Ok(animals);
    }

    // GET: api/Animals/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAnimal(int id)
    {
        var animal = await _context.Animals
            .Include(a => a.Photos) // Include photos
            .FirstOrDefaultAsync(a => a.AnimalId == id);

        if (animal == null)
        {
            return NotFound();
        }

        return Ok(MapToAnimalResponseDto(animal));
    }

    //// PUT: api/Animals/5
    //[HttpPut("{id}")]
    //public async Task<IActionResult> UpdateAnimal(int id, [FromForm] AnimalUpdateDto animalDto)
    //{
    //    var animal = await _context.Animals
    //        .Include(a => a.Photos) // Include photos
    //        .FirstOrDefaultAsync(a => a.AnimalId == id);

    //    if (animal == null)
    //    {
    //        return NotFound();
    //    }

    //    animal.Description = animalDto.Description;
    //    animal.Age = animalDto.Age;
    //    animal.Gender = animalDto.Gender;
    //    animal.HealthIssues = animalDto.HealthIssues;

    //    // Update photos if new photos are provided
    //    if (animalDto.Photos != null && animalDto.Photos.Count > 0)
    //    {
    //        // Remove existing photos
    //        _context.Photos.RemoveRange(animal.Photos);

    //        foreach (var photo in animalDto.Photos)
    //        {
    //            using var memoryStream = new MemoryStream();
    //            await photo.CopyToAsync(memoryStream);
    //            var photoBytes = memoryStream.ToArray();

    //            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
    //            var filePath = Path.Combine(_env.WebRootPath, "images", fileName);
    //            await System.IO.File.WriteAllBytesAsync(filePath, photoBytes);

    //            animal.Photos.Add(new Photo
    //            {
    //                ImageData = photoBytes,
    //                ImageUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}",
    //                AnimalId = animal.AnimalId // Associate the photo with the animal
    //            });
    //        }
    //    }

    //    _context.Animals.Update(animal);
    //    await _context.SaveChangesAsync();

    //    return Ok(MapToAnimalResponseDto(animal));
    //}

    // DELETE: api/Animals/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAnimal(int id)
    {
        var animal = await _context.Animals
            .Include(a => a.Photos) // Include photos
            .FirstOrDefaultAsync(a => a.AnimalId == id);

        if (animal == null)
        {
            return NotFound();
        }

        // Delete associated photos
        _context.Photos.RemoveRange(animal.Photos);
        _context.Animals.Remove(animal);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private AnimalResponseDto MapToAnimalResponseDto(Animal animal)
    {
        return new AnimalResponseDto
        {
            AnimalId = animal.AnimalId,
            Description = animal.Description,
            Age = animal.Age,
            Gender = animal.Gender,
            HealthIssues = animal.HealthIssues,
            PhotoUrls = animal.Photos.Select(p => p.ImageUrl).ToList() // Include photo URLs
        };
    }
}