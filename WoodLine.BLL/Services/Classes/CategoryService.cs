using WoodLine.BLL.Services.Interfaces;
using WoodLine.DAL.DTOs.Requests.Admin;
using WoodLine.DAL.DTOs.Responses.Admin;
using WoodLine.DAL.DTOs.Responses.User;
using WoodLine.DAL.Models;
using WoodLine.DAL.Repositories.Interfaces;
using Mapster;

namespace WoodLine.BLL.Services.Classes;

public class CategoryService : ICategoryService
{
    private const string ArabicLanguagePrefix = "ar";
    private readonly ICategoryRepository _categoryRepository;
    private readonly IFileService _fileService;

    public CategoryService(ICategoryRepository categoryRepository, IFileService fileService)
    {
        _categoryRepository = categoryRepository;
        _fileService = fileService;
    }

    public async Task<IEnumerable<UserCategoryResponse>> GetAllCategoriesForUserAsync(string language)
    {
        var categories = (await _categoryRepository.GetAllAsync(c => c.Status == Status.Active, includeProperties: "Products,Products.SubImages")).ToList();
        var result = new List<UserCategoryResponse>();

        foreach (var category in categories)
        {
            category.Products = category.Products
                .Where(p => p.Status == Status.Active)
                .ToList();

            result.Add(MapUserCategory(category, language));
        }

        return result;
    }

    public async Task<IEnumerable<AdminCategoryResponse>> GetAllCategoriesForAdminAsync(string language)
    {
        var categories = (await _categoryRepository.GetAllAsync(includeProperties: "Products,Products.SubImages")).ToList();
        var result = new List<AdminCategoryResponse>();

        foreach (var category in categories)
        {
            result.Add(MapAdminCategory(category, language));
        }

        return result;
    }

    public async Task<UserCategoryResponse?> GetCategoryByIdForUserAsync(string id, string language)
    {
        var category = await _categoryRepository.GetByIdAsync(id, includeProperties: "Products,Products.SubImages");
        if (category == null || category.Status != Status.Active) return null;

        category.Products = category.Products
            .Where(p => p.Status == Status.Active)
            .ToList();

        return MapUserCategory(category, language);
    }

    public async Task<AdminCategoryResponse?> GetCategoryByIdForAdminAsync(string id, string language)
    {
        var category = await _categoryRepository.GetByIdAsync(id, includeProperties: "Products,Products.SubImages");
        return category == null ? null : MapAdminCategory(category, language);
    }

    public async Task<AdminCategoryResponse> CreateCategoryAsync(AdminCreateCategoryRequest request)
    {
        // Enforce unique category name (case-insensitive)
        var allCategories = await _categoryRepository.GetAllAsync();
        if (allCategories.Any(c => string.Equals(c.Name, request.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("A category with this name already exists.");
        }

        var category = request.Adapt<Category>();
        // Removed manual Id generation; assuming DB/EF handles it.
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;
        category.Image = await _fileService.UploadFileAsync(request.Image, "categories");

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveAsync();

        return category.Adapt<AdminCategoryResponse>();
    }

    public async Task<bool> UpdateCategoryAsync(string id, AdminUpdateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) return false;

        // If changing name, ensure uniqueness
        if (request.Name != null)
        {
            var allCategories = await _categoryRepository.GetAllAsync();
            if (allCategories.Any(c => c.Id != id && string.Equals(c.Name, request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("A category with this name already exists.");
            }
            category.Name = request.Name;
        }

        if (request.Status.HasValue) category.Status = request.Status.Value;

        category.UpdatedAt = DateTime.UtcNow;

        if (request.Image != null)
        {
            // Delete old image if exists
            if (!string.IsNullOrEmpty(category.Image))
            {
                var oldFileName = Path.GetFileName(category.Image);
                _fileService.DeleteFile(oldFileName, "categories");
            }
            category.Image = await _fileService.UploadFileAsync(request.Image, "categories");
        }

        _categoryRepository.Update(category);
        return await _categoryRepository.SaveAsync();
    }

    public async Task<bool> DeleteCategoryAsync(string id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) return false;

        // Delete physical image
        if (!string.IsNullOrEmpty(category.Image))
        {
            var oldFileName = Path.GetFileName(category.Image);
            _fileService.DeleteFile(oldFileName, "categories");
        }

        _categoryRepository.Delete(category);
        return await _categoryRepository.SaveAsync();
    }

    public async Task<bool> ToggleCategoryStatusAsync(string id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) return false;
        category.Status = category.Status == Status.Active ? Status.Inactive : Status.Active;
        category.UpdatedAt = DateTime.UtcNow;
        _categoryRepository.Update(category);
        return await _categoryRepository.SaveAsync();
    }

    private static bool IsArabic(string language) =>
        !string.IsNullOrWhiteSpace(language) && language.StartsWith(ArabicLanguagePrefix, StringComparison.OrdinalIgnoreCase);

    private static UserCategoryResponse MapUserCategory(Category category, string language)
    {
        var products = category.Products;
        return new UserCategoryResponse
        {
            Id = category.Id,
            Name = IsArabic(language) ? category.NameAr : category.Name,
            Image = category.Image,
            Products = products.Select(product => new UserProductResponse
            {
                Id = product.Id,
                Name = IsArabic(language) ? product.NameAr : product.Name,
                Description = IsArabic(language) ? product.DescriptionAr : product.Description,
                Price = product.Price,
                Discount = product.Discount,
                MainImage = product.MainImage,
                Rate = product.Rate,
                CategoryId = product.CategoryId,
                SubImages = product.SubImages.Select(si => si.ImageName).ToList(),
                Reviews = new List<UserReviewResponse>()
            }).ToList()
        };
    }

    private static AdminCategoryResponse MapAdminCategory(Category category, string language)
    {
        var products = category.Products;
        return new AdminCategoryResponse
        {
            Id = category.Id,
            Name = IsArabic(language) ? category.NameAr : category.Name,
            Image = category.Image,
            Status = category.Status,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            Products = products.Select(product => new AdminProductResponse
            {
                Id = product.Id,
                Name = IsArabic(language) ? product.NameAr : product.Name,
                Description = IsArabic(language) ? product.DescriptionAr : product.Description,
                Price = product.Price,
                Discount = product.Discount,
                Quantity = product.Quantity,
                MainImage = product.MainImage,
                Rate = product.Rate,
                CategoryId = product.CategoryId,
                Status = product.Status,
                SubImages = product.SubImages.Select(si => si.ImageName).ToList(),
                Reviews = new List<AdminReviewResponse>(),
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            }).ToList()
        };
    }
}
