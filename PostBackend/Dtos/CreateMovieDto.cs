using System.ComponentModel.DataAnnotations;

namespace PostBackend.Dtos;

public class CreateMovieDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    // Optional
    [MaxLength(100)]
    public string? Genre { get; set; }

    // Optional, 1-5
    [Range(1, 5)]
    public int? Rating { get; set; }

    // Optional poster image URL
    public string? PosterImage { get; set; }
}

