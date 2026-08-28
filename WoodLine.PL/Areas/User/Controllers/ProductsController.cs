using WoodLine.BLL.Services.Interfaces;
using WoodLine.DAL.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WoodLine.PL.Utilities;

namespace WoodLine.PL.Areas.User.Controllers;

[Area("User")]
[Route("api/[area]/[controller]")]
[ApiController]
[AllowAnonymous]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryRequest query)
    {
        var language = LanguageHelper.GetLanguageFromHeader(Request);
        var products = await _productService.GetProductsForUserAsync(query, language);
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        products.Items.NormalizeProductImages(baseUrl);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var language = LanguageHelper.GetLanguageFromHeader(Request);
        var product = await _productService.GetProductByIdForUserAsync(id, language);
        if (product == null) return NotFound();
        product.NormalizeProductImages($"{Request.Scheme}://{Request.Host}");
        return Ok(product);
    }
}
