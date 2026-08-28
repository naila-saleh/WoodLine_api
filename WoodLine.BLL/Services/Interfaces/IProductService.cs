using WoodLine.DAL.DTOs.Requests.Admin;
using WoodLine.DAL.DTOs.Requests;
using WoodLine.DAL.DTOs.Responses.Admin;
using WoodLine.DAL.DTOs.Responses.User;
using WoodLine.DAL.DTOs.Responses;
using Microsoft.AspNetCore.Http;

namespace WoodLine.BLL.Services.Interfaces;

public interface IProductService
{
    Task<PaginatedResponse<UserProductResponse>> GetProductsForUserAsync(ProductQueryRequest query, string language);
    Task<PaginatedResponse<AdminProductResponse>> GetProductsForAdminAsync(ProductQueryRequest query, string language);
    Task<UserProductResponse?> GetProductByIdForUserAsync(string id, string language);
    Task<AdminProductResponse?> GetProductByIdForAdminAsync(string id, string language);
    Task<AdminProductResponse> CreateProductAsync(AdminCreateProductRequest request);
    Task<bool> UpdateProductAsync(string id, AdminUpdateProductRequest request);
    Task<bool> DeleteProductAsync(string id);
    Task<bool> ToggleProductStatusAsync(string id);
    
    // Image management endpoints
    Task<bool> UpdateMainImageAsync(string productId, IFormFile mainImage);
    Task<bool> DeleteMainImageAsync(string productId);
    Task<bool> AddSubImagesAsync(string productId, IFormFileCollection subImages);
    Task<bool> DeleteSubImageAsync(string productId, string subImageId);
}
