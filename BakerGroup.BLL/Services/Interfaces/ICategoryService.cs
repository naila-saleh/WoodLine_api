using BakerGroup.DAL.DTOs.Requests.Admin;
using BakerGroup.DAL.DTOs.Responses.Admin;
using BakerGroup.DAL.DTOs.Responses.User;

namespace BakerGroup.BLL.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<UserCategoryResponse>> GetAllCategoriesForUserAsync(string language);
    Task<IEnumerable<AdminCategoryResponse>> GetAllCategoriesForAdminAsync(string language);
    Task<UserCategoryResponse?> GetCategoryByIdForUserAsync(string id, string language);
    Task<AdminCategoryResponse?> GetCategoryByIdForAdminAsync(string id, string language);
    Task<AdminCategoryResponse> CreateCategoryAsync(AdminCreateCategoryRequest request);
    Task<bool> UpdateCategoryAsync(string id, AdminUpdateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(string id);
    Task<bool> ToggleCategoryStatusAsync(string id);
}
