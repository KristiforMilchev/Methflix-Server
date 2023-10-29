using Domain.Dtos;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("/API/V1/[controller]")]
[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _movieRepository;

    public MoviesController(IMovieRepository repository)
    {
        _movieRepository = repository;
    }

    [HttpGet("Category/{id}")]
    public async Task<IActionResult> GetCategoryMovies(int id)
    {
        var result = await _movieRepository.GetCategoryMovies(id);
        return result == null ? StatusCode(500) : Ok(result);
    }

    [HttpGet("Categories")]
    public async Task<IActionResult> GetCategoriesMovies()
    {
        var result = await _movieRepository.GetCategoryWithMovies();
        
        return Ok(
            result.Select(
                async x => new CategoryResponseDto
                {
                    Id = x.Id,
                    Movies = x.Movies.Select(
                        y => new MovieResponseDto
                        {
                            Name = y.Name,
                            Id = y.Id,
                            Lenght = y.TimeData,
                            Thumbnail = y.Thumbnail ?? string.Empty
                        }
                    ).ToList(),
                    Name = x.Name,
                    TvShows = await _movieRepository.GetCategoryTvShows(x.Id)
                }
            ).ToList()    
        );
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMovieById(int id)
    {
        var movie = await _movieRepository.GetMovieById(id);
        return movie == null ? StatusCode(500) : Ok(new MovieResponseDto
        {
            Id = movie.Id,
            Name = movie.Name,
            Lenght = movie.TimeData,
            Thumbnail = movie.Thumbnail ?? string.Empty
        });
    }

    [HttpPost("Upload-Movie-Thumbail")]
    public async Task<IActionResult> UploadMovieThumnail([FromForm] UploadMovieThumbnail responseDto)
    {
        var movie = await _movieRepository.GetMovieById(responseDto.Id);
        if (movie == null) return StatusCode(500);
        
        using var memoryStream = new MemoryStream();
        await responseDto.File.CopyToAsync(memoryStream); 
        var data = memoryStream.ToArray();
        movie.Thumbnail = Convert.ToBase64String(data);
        var result = await _movieRepository.UpdateMovie(movie);

        return result ? StatusCode(500) : Ok();
    }
}
