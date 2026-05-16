using BakerGroup.BLL.Services.Interfaces;
using BakerGroup.DAL.DTOs.Requests.User;
using BakerGroup.DAL.DTOs.Responses.Admin;
using BakerGroup.DAL.DTOs.Responses.User;
using BakerGroup.DAL.Models;
using BakerGroup.DAL.Repositories.Interfaces;
using Mapster;

namespace BakerGroup.BLL.Services.Classes;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<IEnumerable<AdminReviewResponse>> GetAllReviewsForAdminAsync()
    {
        var reviews = await _reviewRepository.GetAllAsync(includeProperties: "User");
        return reviews.Select(r =>
        {
            var res = r.Adapt<AdminReviewResponse>();
            res.UserName = r.User?.UserName ?? "Unknown";
            return res;
        });
    }

    public async Task<IEnumerable<UserReviewResponse>> GetProductReviewsAsync(string productId)
    {
        var reviews = await _reviewRepository.GetAllAsync(r => r.ProductId == productId, includeProperties: "User");
        return reviews.Select(r =>
        {
            var res = r.Adapt<UserReviewResponse>();
            res.UserName = r.User?.UserName ?? "Unknown";
            return res;
        });
    }

    public async Task<IEnumerable<UserReviewResponse>> GetUserReviewsAsync(string userId)
    {
        var reviews = await _reviewRepository.GetAllAsync(r => r.UserId == userId, includeProperties: "User");
        return reviews.Select(r =>
        {
            var res = r.Adapt<UserReviewResponse>();
            res.UserName = r.User?.UserName ?? "Unknown";
            return res;
        });
    }

    public async Task<UserReviewResponse> CreateReviewAsync(UserCreateReviewRequest request, string userId)
    {
        var review = request.Adapt<Review>();
        review.UserId = userId;
        review.CreatedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;
        review.Status = Status.Active;

        await _reviewRepository.AddAsync(review);
        await _reviewRepository.SaveAsync();

        var response = review.Adapt<UserReviewResponse>();
        // We'd normally fetch the username here or pass it in
        return response;
    }

    public async Task<bool> DeleteReviewAsync(string id, string userId, bool isAdmin)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null) return false;

        if (!isAdmin && review.UserId != userId)
        {
            return false;
        }

        _reviewRepository.Delete(review);
        return await _reviewRepository.SaveAsync();
    }

    public async Task<bool> ToggleReviewStatusAsync(string id)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null) return false;
        review.Status = review.Status == Status.Active ? Status.Inactive : Status.Active;
        review.UpdatedAt = DateTime.UtcNow;
        _reviewRepository.Update(review);
        return await _reviewRepository.SaveAsync();
    }
}
