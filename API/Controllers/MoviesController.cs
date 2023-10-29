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
                    Movies = x.Movies.Where(x=>x.TvShowId == null).Select(
                        y => new MovieResponseDto
                        {
                            Name = y.Name,
                            Id = y.Id,
                            Lenght = y.TimeData,
                            Thumbnail = y.Thumbnail ?? string.Empty
                        }
                    ).ToList(),
                    Name = x.Name,
                    TvShows = _movieRepository.GetCategoryTvShows(x.Id).ConfigureAwait(true).GetAwaiter().GetResult().Select(z=> new TvShowDto
                    {
                        Id  = z.Id,
                        Name = z.Name,
                        Thumbnail = "",
                        Seasons = z.Season ?? 1,
                        MovieCount = z.Movies.Count,
                    }).ToList()
                }
            ).ToList()    
        );
    }

    [HttpGet("Tv-Show-Episodes/{id}")]
    public async Task<IActionResult> TvShowEpisodes(int id)
    {
        var episodes = await _movieRepository.GetTvShowEpisodesById(id);
        if (episodes.Seasons.Count == 0) return StatusCode(500);

        return Ok(episodes);
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
