using BakerGroup.DAL.DTOs.Requests.User;
using BakerGroup.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BakerGroup.PL.Areas.User.Controllers;

[Area("User")]
[Route("api/[area]/[controller]")]
[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var reviews = await _reviewService.GetAllReviewsForAdminAsync(); // User can view all reviews too
        return Ok(reviews);
    }

    [HttpGet("product/{productId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByProduct(string productId)
    {
        var reviews = await _reviewService.GetProductReviewsAsync(productId);
        return Ok(reviews);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyReviews()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var reviews = await _reviewService.GetUserReviewsAsync(userId);
        return Ok(reviews);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(UserCreateReviewRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var review = await _reviewService.CreateReviewAsync(request, userId);
        return Ok(review);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "User,Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _reviewService.DeleteReviewAsync(id, userId, false);
        if (!result) return NotFound("Review not found or you don't have permission to delete it.");
        return NoContent();
    }
}
