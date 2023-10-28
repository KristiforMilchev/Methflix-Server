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
                x => new CategoryResponseDto
                {
                    Id = x.Id,
                    Movies = x.Movies.Select(
                        y => new MovieResponseDto
                        {
                            Name = y.Name,
                            Id = y.Id,
                            Lenght = y.TimeData,
                            Thumbnail = ""
                        }
                    ).ToList(),
                    Name = x.Name
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
            Thumbnail = ""
        });
    }
}
