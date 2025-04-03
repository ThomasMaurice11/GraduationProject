using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.SqlServer.Server;

public class AnimalResponseDto
{
    public int AnimalId { get; set; }
    public string Description { get; set; }

    public List<string> PhotoUrls { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; }
    public string HealthIssues { get; set; }
}
