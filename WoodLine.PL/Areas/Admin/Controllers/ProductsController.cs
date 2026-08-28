using WoodLine.DAL.DTOs.Requests.Admin;
using WoodLine.DAL.DTOs.Requests;
using WoodLine.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WoodLine.PL.Utilities;

namespace WoodLine.PL.Areas.Admin.Controllers;

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
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryRequest query)
    {
        var language = LanguageHelper.GetLanguageFromHeader(Request);
        var paginatedProducts = await _productService.GetProductsForAdminAsync(query, language);
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        paginatedProducts.Items.NormalizeProductImages(baseUrl);
        return Ok(paginatedProducts);
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

    [HttpPut("{id}/main-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateMainImage(string id, [FromForm] AdminUpdateMainImageRequest request)
    {
        if (request.MainImage == null || request.MainImage.Length == 0)
            return BadRequest("Main image is required");

        var result = await _productService.UpdateMainImageAsync(id, request.MainImage);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}/main-image")]
    public async Task<IActionResult> DeleteMainImage(string id)
    {
        var result = await _productService.DeleteMainImageAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/sub-images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddSubImages(string id, [FromForm] AdminAddSubImagesRequest request)
    {
        if (request.SubImages == null || request.SubImages.Count == 0)
            return BadRequest("At least one sub-image is required");

        var result = await _productService.AddSubImagesAsync(id, request.SubImages);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}/sub-images/{subImageId}")]
    public async Task<IActionResult> DeleteSubImage(string id, string subImageId)
    {
        var result = await _productService.DeleteSubImageAsync(id, subImageId);
        if (!result) return NotFound();
        return NoContent();
    }
}
