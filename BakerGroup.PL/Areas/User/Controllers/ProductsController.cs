using BakerGroup.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BakerGroup.PL.Areas.User.Controllers;

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
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllProductsForUserAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var product = await _productService.GetProductByIdForUserAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }
}
