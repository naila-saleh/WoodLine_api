using BakerGroup.DAL.DTOs.Requests.User;
using BakerGroup.DAL.DTOs.Responses.Admin;
using BakerGroup.DAL.DTOs.Responses.User;

namespace BakerGroup.BLL.Services.Interfaces;

public interface IReviewService
{
    Task<IEnumerable<AdminReviewResponse>> GetAllReviewsForAdminAsync();
    Task<IEnumerable<UserReviewResponse>> GetProductReviewsAsync(string productId);
    Task<IEnumerable<UserReviewResponse>> GetUserReviewsAsync(string userId);
    Task<UserReviewResponse> CreateReviewAsync(UserCreateReviewRequest request, string userId);
    Task<bool> DeleteReviewAsync(string id, string userId, bool isAdmin);
    Task<bool> ToggleReviewStatusAsync(string id);
}
