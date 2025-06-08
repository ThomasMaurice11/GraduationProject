using GP.Models;
using GP.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GP;

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
            throw new AppException("No photos uploaded.", 400, "Bad Request");

        try
        {
            var animal = new Animal
            {
                Title = animalDto.Title,
                Description = animalDto.Description,
                Age = animalDto.Age,
                Gender = animalDto.Gender,
                HealthIssues = animalDto.HealthIssues,
                FoundDate = animalDto.FoundDate,
                CreationDate = DateTime.UtcNow // Set creation date to current UTC time
            };

            _context.Animals.Add(animal);
            await _context.SaveChangesAsync();

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
                    AnimalId = animal.AnimalId,
                    PetId = null
                });
            }

            await _context.SaveChangesAsync();
            return Ok(MapToAnimalResponseDto(animal));
        }
        catch (Exception ex)
        {
            throw new AppException($"Failed to create animal: {ex.Message}", 500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAnimals()
    {
        try
        {
            var animals = await _context.Animals
                .Include(a => a.Photos)
                .ToListAsync();

            var result = animals.Select(a => MapToAnimalResponseDto(a));
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw new AppException($"Error fetching animals: {ex.Message}", 500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAnimal(int id)
    {
        var animal = await _context.Animals
            .Include(a => a.Photos)
            .FirstOrDefaultAsync(a => a.AnimalId == id);

        if (animal == null)
            throw new AppException($"Animal with ID {id} not found.", 404, "Not Found");

        return Ok(MapToAnimalResponseDto(animal));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAnimal(int id)
    {
        var animal = await _context.Animals
            .Include(a => a.Photos)
            .FirstOrDefaultAsync(a => a.AnimalId == id);

        if (animal == null)
            throw new AppException($"Animal with ID {id} not found.", 404, "Not Found");

        try
        {
            _context.Photos.RemoveRange(animal.Photos);
            _context.Animals.Remove(animal);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            throw new AppException($"Error deleting animal: {ex.Message}", 500);
        }
    }

    private AnimalResponseDto MapToAnimalResponseDto(Animal animal)
    {
        return new AnimalResponseDto
        {
            AnimalId = animal.AnimalId,
            Title = animal.Title,
            Description = animal.Description,
            Age = animal.Age,
            Gender = animal.Gender,
            HealthIssues = animal.HealthIssues,
            FoundDate = animal.FoundDate,
            CreationDate = animal.CreationDate,
            PhotoUrls = animal.Photos.Select(p => p.ImageUrl).ToList()
        };
    }
}