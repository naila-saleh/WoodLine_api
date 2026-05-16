using BakerGroup.DAL.DTOs.Requests.Admin;
using BakerGroup.DAL.DTOs.Responses.Admin;
using BakerGroup.DAL.DTOs.Responses.User;

namespace BakerGroup.BLL.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<UserCategoryResponse>> GetAllCategoriesForUserAsync();
    Task<IEnumerable<AdminCategoryResponse>> GetAllCategoriesForAdminAsync();
    Task<UserCategoryResponse?> GetCategoryByIdForUserAsync(string id);
    Task<AdminCategoryResponse?> GetCategoryByIdForAdminAsync(string id);
    Task<AdminCategoryResponse> CreateCategoryAsync(AdminCreateCategoryRequest request);
    Task<bool> UpdateCategoryAsync(string id, AdminUpdateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(string id);
    Task<bool> ToggleCategoryStatusAsync(string id);
}
