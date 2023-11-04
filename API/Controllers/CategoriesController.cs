using Domain.Dtos;
using Domain.Models;
using FFmpeg.AutoGen;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("/API/V1/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoriesController(ICategoryRepository repository)
    {
        _categoryRepository = repository;
    }
    
    [HttpGet("Get-All")]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _categoryRepository.GetCategories();
        return Ok(categories.Select(x=> new CategoryResponseDto
        {
            Id = x.Id,
            Movies = new List<MovieResponseDto>(),
            Name = x.Name
        }).ToList());
    }

    [HttpGet("Get-Category/{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _categoryRepository.GetCategory(id);
        return category == null ? StatusCode(500) : Ok(new CategoryResponseDto
        {
            Id = category.Id,
            Movies = new List<MovieResponseDto>(),
            Name = category.Name
        });
    }

    [HttpPost("Update/{category}")]
    public async Task<IActionResult> UpdateCategory([FromBody] Category category)
    {
        var result = await _categoryRepository.UpdateCategory(category);
        return result ? Ok() : StatusCode(500);
    }

    [HttpPost("Add/{category}")]
    public async Task<IActionResult> AddCategory([FromBody] Category category)
    {
        var result = await _categoryRepository.AddCategory(category);
        return result ? Ok() : StatusCode(500);
    }

    [HttpPost("Delete/{category}")]
    public async Task<IActionResult> DeleteCategory([FromBody] int category)
    {
        var result = await _categoryRepository.RemoveCategory(category);
        return result ? Ok() : StatusCode(500);
    }
}
