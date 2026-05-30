using BakerGroup.BLL.Services.Interfaces;
using BakerGroup.DAL.DTOs.Requests.Admin;
using BakerGroup.DAL.DTOs.Responses.Admin;
using BakerGroup.DAL.DTOs.Responses.User;
using BakerGroup.DAL.Models;
using BakerGroup.DAL.Repositories.Interfaces;
using Mapster;

namespace BakerGroup.BLL.Services.Classes;

public class ProductService : IProductService
{
    private const string ArabicLanguagePrefix = "ar";
    private readonly IProductRepository _productRepository;
    private readonly IFileService _fileService;

    public ProductService(IProductRepository productRepository, IFileService fileService)
    {
        _productRepository = productRepository;
        _fileService = fileService;
    }

    public async Task<IEnumerable<UserProductResponse>> GetAllProductsForUserAsync(string language)
    {
        var products = await _productRepository.GetAllAsync(p => p.Status == Status.Active, includeProperties: "SubImages,Reviews.User");
        return products.Select(p => MapUserProduct(p, language));
    }

    public async Task<IEnumerable<AdminProductResponse>> GetAllProductsForAdminAsync(string language)
    {
        var products = await _productRepository.GetAllAsync(includeProperties: "SubImages,Reviews.User");
        return products.Select(p => MapAdminProduct(p, language));
    }

    public async Task<UserProductResponse?> GetProductByIdForUserAsync(string id, string language)
    {
        var product = await _productRepository.GetByIdAsync(id, includeProperties: "SubImages,Reviews.User");
        if (product == null || product.Status != Status.Active) return null;

        return MapUserProduct(product, language);
    }

    public async Task<AdminProductResponse?> GetProductByIdForAdminAsync(string id, string language)
    {
        var product = await _productRepository.GetByIdAsync(id, includeProperties: "SubImages,Reviews.User");
        if (product == null) return null;

        return MapAdminProduct(product, language);
    }

        public async Task<AdminProductResponse> CreateProductAsync(AdminCreateProductRequest request)
    {
        var product = request.Adapt<Product>();

        // Mapster may try to map IFormFileCollection -> List<ProductImage> producing empty
        // ProductImage instances (ImageName == null). Ensure we don't persist such entries.
        product.SubImages = product.SubImages?
            .Where(si => !string.IsNullOrWhiteSpace(si.ImageName))
            .ToList() ?? new List<ProductImage>();

        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        // Make discount optional: default to 0 if not provided
        product.Discount = request.Discount ?? 0m;

        if (request.MainImage is not null)
        {
            product.MainImage = await _fileService.UploadFileAsync(request.MainImage, "products/main");
        }

        if (request.SubImages != null && request.SubImages.Count > 0)
        {
            var subImagePaths = await _fileService.UploadFilesAsync(request.SubImages, "products/sub");
            foreach (var path in subImagePaths)
            {
                product.SubImages.Add(new ProductImage
                {
                    ImageName = path,
                    ProductId = product.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = Status.Active
                });
            }
        }

        await _productRepository.AddAsync(product);
        await _productRepository.SaveAsync();

        var result = product.Adapt<AdminProductResponse>();
        result.SubImages = product.SubImages.Select(si => si.ImageName).ToList();
        return result;
    }

    public async Task<bool> UpdateProductAsync(string id, AdminUpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id, includeProperties: "SubImages");
        if (product == null) return false;

        request.Adapt(product);

        // After mapping, remove any placeholder sub-images that don't have ImageName set
        product.SubImages = product.SubImages?
            .Where(si => !string.IsNullOrWhiteSpace(si.ImageName))
            .ToList() ?? new List<ProductImage>();
        // Ensure collections aren't null after mapping
        product.UpdatedAt = DateTime.UtcNow;

        // Apply optional discount if provided
        if (request.Discount.HasValue)
        {
            product.Discount = request.Discount.Value;
        }

        if (request.MainImage != null)
        {
            if (!string.IsNullOrEmpty(product.MainImage))
            {
                var oldFileName = Path.GetFileName(product.MainImage);
                _fileService.DeleteFile(oldFileName, "products/main");
            }
            product.MainImage = await _fileService.UploadFileAsync(request.MainImage, "products/main");
        }

        if (request.SubImages != null && request.SubImages.Count > 0)
        {
            var subImagePaths = await _fileService.UploadFilesAsync(request.SubImages, "products/sub");
            foreach (var path in subImagePaths)
            {
                product.SubImages.Add(new ProductImage
                {
                    ImageName = path,
                    ProductId = product.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = Status.Active
                });
            }
        }

        _productRepository.Update(product);
        return await _productRepository.SaveAsync();
    }

    public async Task<bool> DeleteProductAsync(string id)
    {
        var product = await _productRepository.GetByIdAsync(id, includeProperties: "SubImages");
        if (product == null) return false;

        if (!string.IsNullOrEmpty(product.MainImage))
        {
            var oldFileName = Path.GetFileName(product.MainImage);
            _fileService.DeleteFile(oldFileName, "products/main");
        }

        foreach (var existingSubImage in product.SubImages)
        {
            var oldFileName = Path.GetFileName(existingSubImage.ImageName);
            _fileService.DeleteFile(oldFileName, "products/sub");
        }

        _productRepository.Delete(product);
        return await _productRepository.SaveAsync();
    }

    public async Task<bool> ToggleProductStatusAsync(string id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return false;
        product.Status = product.Status == Status.Active ? Status.Inactive : Status.Active;
        product.UpdatedAt = DateTime.UtcNow;
        _productRepository.Update(product);
        return await _productRepository.SaveAsync();
    }

    private static bool IsArabic(string language) =>
        !string.IsNullOrWhiteSpace(language) && language.StartsWith(ArabicLanguagePrefix, StringComparison.OrdinalIgnoreCase);

    private static UserProductResponse MapUserProduct(Product product, string language)
    {
        return new UserProductResponse
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
            Reviews = product.Reviews
                .Where(r => r.Status == Status.Active)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new UserReviewResponse
                {
                    Id = r.Id,
                    Rate = r.Rate,
                    Comment = r.Comment,
                    UserName = r.User.UserName ?? "Unknown",
                    ProductId = r.ProductId,
                    CreatedAt = r.CreatedAt
                })
                .ToList()
        };
    }

    private static AdminProductResponse MapAdminProduct(Product product, string language)
    {
        return new AdminProductResponse
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
            Reviews = product.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new AdminReviewResponse
                {
                    Id = r.Id,
                    Rate = r.Rate,
                    Comment = r.Comment,
                    ProductId = r.ProductId,
                    UserId = r.UserId,
                    UserName = r.User.UserName ?? "Unknown",
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                })
                .ToList(),
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
