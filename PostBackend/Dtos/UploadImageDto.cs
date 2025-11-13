using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PostBackend.Dtos;

public class UploadImageDto
{
    [Required]
    public IFormFile File { get; set; } = default!;
}
