using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GP.Models;
using System.ComponentModel.DataAnnotations;
using GP.DTOs.Post;
using DocumentFormat.OpenXml.Presentation;
using GP.DTOs.Pet;
using Microsoft.AspNetCore.Authorization;
using GP.DTOs;

namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PostsController(AuthDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.Photos)
                .OrderByDescending(p => p.CreationDate)
                .ToListAsync();

            return posts.Select(MapToPostResponseDto).ToList();
        }
        // GET: api/Posts/5
        // Change return type from Post to PostResponseDto
        [HttpGet("{id}")]
        public async Task<ActionResult<PostResponseDto>> GetPost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            return MapToPostResponseDto(post);
        }

        // GET: api/Posts/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetPostsByUser(string userId)
        {
            return await _context.Posts
                .Where(p => p.UserId == userId)
                .Include(p => p.Photos)
                .Include(p => p.Owner)
                .OrderByDescending(p => p.CreationDate)
                .Select(p => MapToPostResponseDto(p))
                .ToListAsync();
        }

        // POST: api/Posts
        [HttpPost]
        public async Task<ActionResult<PostResponseDto>> CreatePost(PostDto postDto)
        {
            var userId = User?.FindFirst("id")?.Value;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var post = new Post
            {
                Title = postDto.Title,
                Description = postDto.Description,
                Breed = postDto.Breed,
                UserId = userId,
                Age = postDto.Age,
                LostDate = postDto.LostDate,
                LostLocation = postDto.LostLocation,
                Gender = postDto.Gender,
                CreationDate = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();


            foreach (var photo in postDto.Photos)
            {
                using var memoryStream = new MemoryStream();
                await photo.CopyToAsync(memoryStream);
                var photoBytes = memoryStream.ToArray();

                var fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
                var filePath = Path.Combine(_env.WebRootPath, "images", fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, photoBytes);

                post.Photos.Add(new Photo
                {
                    ImageData = photoBytes,
                    ImageUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}",
                    PostId = post.PostId,
                });
            }

            await _context.SaveChangesAsync();

            // Load the post with owner data
            var createdPost = await _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.PostId == post.PostId);

            return CreatedAtAction(nameof(GetPost),
                new { id = post.PostId },
                MapToPostResponseDto(createdPost));
        }

        // PUT: api/Posts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, PostDto postDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            post.Title = postDto.Title;
            post.Description = postDto.Description;
            post.Breed = postDto.Breed;
            post.Age = postDto.Age;
            post.LostDate = postDto.LostDate;
            post.LostLocation = postDto.LostLocation;
            post.Gender = postDto.Gender;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
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

        // DELETE: api/Posts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            // First delete all photos associated with the post
            _context.Photos.RemoveRange(post.Photos);

            // Then delete the post itself
            _context.Posts.Remove(post);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Posts/search?query=...
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<PostResponseDto>>> SearchPosts([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }

            // 1. First load the complete data from database
            var posts = await _context.Posts
                .Where(p => p.Title.Contains(query) ||
                           p.Description.Contains(query) ||
                           p.Breed.Contains(query) ||
                           p.LostLocation.Contains(query))
                .Include(p => p.Photos)
                .Include(p => p.Owner)
                .OrderByDescending(p => p.CreationDate)
                .ToListAsync();

            // 2. Map each post to DTO
            var result = posts.Select(p => new PostResponseDto
            {
                PostId = p.PostId,
                Title = p.Title,
                Description = p.Description,
                Breed = p.Breed,
                Age = p.Age,
                LostDate = p.LostDate,
                LostLocation = p.LostLocation,
                Gender = p.Gender,
                CreationDate = p.CreationDate,
                PhotoUrls = p.Photos.Select(p => p.ImageUrl).ToList(),
                Owner = p.Owner != null ? new OwnerDto
                {
                    Id = p.Owner.Id,
                    UserName = p.Owner.UserName,
                    Email = p.Owner.Email
                } : null
            }).ToList();

            return result;
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }

        private PostResponseDto MapToPostResponseDto(Post post)
        {
            return new PostResponseDto
            {
                PostId = post.PostId,
                Title = post.Title,
                Description = post.Description,
                Breed = post.Breed,
                Age = post.Age,
                LostDate = post.LostDate,
                LostLocation = post.LostLocation,
                Gender = post.Gender,
                CreationDate = post.CreationDate,
                PhotoUrls = post.Photos.Select(p => p.ImageUrl).ToList(),
                Owner = post.Owner != null ? new OwnerDto
                {
                    Id = post.Owner.Id,
                    UserName = post.Owner.UserName,
                    Email = post.Owner.Email,
                    PhoneNumber=post.Owner.PhoneNumber
                } : null
            };
        }
    }

   
}