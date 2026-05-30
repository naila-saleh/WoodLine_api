using BakerGroup.BLL.Services.Interfaces;
using BakerGroup.PL.Utilities;
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
        var language = LanguageHelper.GetLanguageFromHeader(Request);
        var categories = (await _categoryService.GetAllCategoriesForUserAsync(language)).ToList();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        categories.NormalizeCategoryImages(baseUrl);

        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var language = LanguageHelper.GetLanguageFromHeader(Request);
        var category = await _categoryService.GetCategoryByIdForUserAsync(id, language);
        if (category == null) return NotFound();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        category.NormalizeCategoryImages(baseUrl);

        return Ok(category);
    }
}
