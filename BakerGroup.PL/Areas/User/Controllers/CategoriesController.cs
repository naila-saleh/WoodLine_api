using BakerGroup.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BakerGroup.PL.Areas.User.Controllers;

[Area("User")]
[Route("api/[area]/[controller]")]
[ApiController]
[AllowAnonymous]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllCategoriesForUserAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var category = await _categoryService.GetCategoryByIdForUserAsync(id);
        if (category == null) return NotFound();
        return Ok(category);
    }
}
