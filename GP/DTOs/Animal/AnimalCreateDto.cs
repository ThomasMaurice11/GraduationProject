﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class AnimalCreateDto
{

    public string Description { get; set; }

    public int Age { get; set; }

    public string Gender { get; set; }

    public string? HealthIssues { get; set; }


    public List<IFormFile> Photos { get; set; }
}
