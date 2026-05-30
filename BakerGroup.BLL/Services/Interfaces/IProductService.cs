using BakerGroup.DAL.DTOs.Requests.Admin;
using BakerGroup.DAL.DTOs.Responses.Admin;
using BakerGroup.DAL.DTOs.Responses.User;

namespace BakerGroup.BLL.Services.Interfaces;

public interface IProductService
{
    Task<IEnumerable<UserProductResponse>> GetAllProductsForUserAsync(string language);
    Task<IEnumerable<AdminProductResponse>> GetAllProductsForAdminAsync(string language);
    Task<UserProductResponse?> GetProductByIdForUserAsync(string id, string language);
    Task<AdminProductResponse?> GetProductByIdForAdminAsync(string id, string language);
    Task<AdminProductResponse> CreateProductAsync(AdminCreateProductRequest request);
    Task<bool> UpdateProductAsync(string id, AdminUpdateProductRequest request);
    Task<bool> DeleteProductAsync(string id);
    Task<bool> ToggleProductStatusAsync(string id);
}
