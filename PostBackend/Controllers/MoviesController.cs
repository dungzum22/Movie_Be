using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostBackend.Data;
using PostBackend.Dtos;
using PostBackend.Models;

namespace PostBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController(AppDbContext db) : ControllerBase
{
    // 1. List movies with search, filter, and sort
    // GET: api/movies?search=abc&genre=Action&sortBy=title|rating&sortOrder=asc|desc
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Movie>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? genre,
        [FromQuery] string sortBy = "title",
        [FromQuery] string sortOrder = "asc")
    {
        IQueryable<Movie> query = db.Movies.AsNoTracking();

        // Search by title
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => m.Title.Contains(search));
        }

        // Filter by genre
        if (!string.IsNullOrWhiteSpace(genre))
        {
            query = query.Where(m => m.Genre != null && m.Genre.Contains(genre));
        }

        // Sort by rating or title
        if (sortBy?.ToLowerInvariant() == "rating")
        {
            query = sortOrder?.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(m => m.Rating ?? 0).ThenBy(m => m.Title)
                : query.OrderBy(m => m.Rating ?? 0).ThenBy(m => m.Title);
        }
        else // Default: sort by title
        {
            query = sortOrder?.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(m => m.Title)
                : query.OrderBy(m => m.Title);
        }

        var items = await query.ToListAsync();
        return Ok(items);
    }

    // GET: api/movies/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Movie>> GetById(int id)
    {
        var movie = await db.Movies.FindAsync(id);
        if (movie is null) return NotFound();
        return Ok(movie);
    }

    // 2. Create a Movie
    [HttpPost]
    public async Task<ActionResult<Movie>> Create([FromBody] CreateMovieDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var movie = new Movie
        {
            Title = dto.Title.Trim(),
            Genre = string.IsNullOrWhiteSpace(dto.Genre) ? null : dto.Genre.Trim(),
            Rating = dto.Rating,
            PosterImage = string.IsNullOrWhiteSpace(dto.PosterImage) ? null : dto.PosterImage,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Movies.Add(movie);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
    }

    // 3. Edit a Movie
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Movie>> Update(int id, [FromBody] UpdateMovieDto dto)
    {
        var movie = await db.Movies.FindAsync(id);
        if (movie is null) return NotFound();

        movie.Title = dto.Title.Trim();
        movie.Genre = string.IsNullOrWhiteSpace(dto.Genre) ? null : dto.Genre.Trim();
        movie.Rating = dto.Rating;
        movie.PosterImage = string.IsNullOrWhiteSpace(dto.PosterImage) ? null : dto.PosterImage;
        movie.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(movie);
    }

    // 4. Delete a Movie
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var movie = await db.Movies.FindAsync(id);
        if (movie is null) return NotFound();
        db.Movies.Remove(movie);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

