using System.ComponentModel.DataAnnotations;

namespace PostBackend.Models;

public class Movie
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    // Optional
    [MaxLength(100)]
    public string? Genre { get; set; }

    // Optional, 1-5
    [Range(1, 5)]
    public int? Rating { get; set; }

    // Optional: URL to the poster image (uploaded or external)
    public string? PosterImage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

