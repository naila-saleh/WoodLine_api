using WoodLine.DAL.DTOs.Requests.Admin;
using WoodLine.DAL.DTOs.Responses.Admin;
using WoodLine.DAL.DTOs.Responses.User;

namespace WoodLine.BLL.Services.Interfaces;

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
