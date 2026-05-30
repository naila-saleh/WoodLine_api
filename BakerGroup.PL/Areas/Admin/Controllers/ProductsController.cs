using BakerGroup.DAL.DTOs.Requests.Admin;
using BakerGroup.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BakerGroup.PL.Utilities;

namespace BakerGroup.PL.Areas.Admin.Controllers;

[Area("Admin")]
[Route("api/[area]/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var language = LanguageHelper.GetLanguageFromHeader(Request);
        var products = (await _productService.GetAllProductsForAdminAsync(language)).ToList();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        products.NormalizeProductImages(baseUrl);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var language = LanguageHelper.GetLanguageFromHeader(Request);
        var product = await _productService.GetProductByIdForAdminAsync(id, language);
        if (product == null) return NotFound();
        product.NormalizeProductImages($"{Request.Scheme}://{Request.Host}");
        return Ok(product);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] AdminCreateProductRequest request)
    {
        var product = await _productService.CreateProductAsync(request);
        product.NormalizeProductImages($"{Request.Scheme}://{Request.Host}");
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPatch("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(string id, [FromForm] AdminUpdateProductRequest request)
    {
        var result = await _productService.UpdateProductAsync(id, request);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _productService.DeleteProductAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPatch("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var result = await _productService.ToggleProductStatusAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
