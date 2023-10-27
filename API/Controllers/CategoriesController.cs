using Domain.Models;
using FFmpeg.AutoGen;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("/api/v1/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoriesController(ICategoryRepository repository)
    {
        _categoryRepository = repository;
    }
    
    [HttpGet("/v1/categories/get-all")]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _categoryRepository.GetCategories();
        return Ok(categories);
    }

    [HttpGet("/v1/categories/get-category/{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _categoryRepository.GetCategory(id);
        return category == null ? StatusCode(500) : Ok(category);
    }

    [HttpPost("/v1/categories/update/{category}")]
    public async Task<IActionResult> UpdateCategory([FromBody] Category category)
    {
        var result = await _categoryRepository.UpdateCategory(category);
        return result ? Ok() : StatusCode(500);
    }

    [HttpPost("/v1/categories/add/{category}")]
    public async Task<IActionResult> AddCategory([FromBody] Category category)
    {
        var result = await _categoryRepository.AddCategory(category);
        return result ? Ok() : StatusCode(500);
    }

    [HttpPost("/v1/categories/delete/{category}")]
    public async Task<IActionResult> DeleteCategory([FromBody] int category)
    {
        var result = await _categoryRepository.RemoveCategory(category);
        return result ? Ok() : StatusCode(500);
    }
}
