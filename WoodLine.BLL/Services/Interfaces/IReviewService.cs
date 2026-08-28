using WoodLine.DAL.DTOs.Requests.User;
using WoodLine.DAL.DTOs.Responses.Admin;
using WoodLine.DAL.DTOs.Responses.User;

namespace WoodLine.BLL.Services.Interfaces;

public interface IReviewService
{
    Task<IEnumerable<AdminReviewResponse>> GetAllReviewsForAdminAsync();
    Task<IEnumerable<UserReviewResponse>> GetProductReviewsAsync(string productId);
    Task<IEnumerable<UserReviewResponse>> GetUserReviewsAsync(string userId);
    Task<UserReviewResponse> CreateReviewAsync(UserCreateReviewRequest request, string userId);
    Task<bool> DeleteReviewAsync(string id, string userId, bool isAdmin);
    Task<bool> ToggleReviewStatusAsync(string id);
}
