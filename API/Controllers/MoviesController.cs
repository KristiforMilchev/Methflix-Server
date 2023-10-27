using Domain.Models;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("/api/v1/[controller]")]
[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _movieRepository;

    public MoviesController(IMovieRepository repository)
    {
        _movieRepository = repository;
    }
    
    [HttpGet("/v1/movies/get-category-movies/{id}")]
    public async Task<IActionResult> GetCategoryMovies(int id)
    {
        var result = await _movieRepository.GetCategoryMovies(id);
        return result == null ? StatusCode(500) : Ok(result);
    }

    [HttpGet("/v1/movies/get-categories-movies")]
    public async Task<IActionResult> GetCategoriesMovies()
    {
        var result = await _movieRepository.GetCategoryWithMovies();
        return Ok(result);
    }
}
