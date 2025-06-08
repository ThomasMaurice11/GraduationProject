

using GP.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Animal
{
    public int AnimalId { get; set; }
    public string Title { get; set; } // Add this line

    [Required]
    public string Description { get; set; }

    //[JsonIgnore]
    //public byte[]? Photo { get; set; } // Store image as binary data

    //public string? PhotoUrl { get; set; } // Store URL for the photo

    [Required]
    public int Age { get; set; }

    public string FoundDate { get; set; }

    [Required]
    public string Gender { get; set; }

    public string? HealthIssues { get; set; }
    public DateTime CreationDate { get; set; }

    public ICollection<Photo> Photos { get; set; } = new List<Photo>();
}



