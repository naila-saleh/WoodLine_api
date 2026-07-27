using BakerGroup.BLL.Services.Interfaces;
using BakerGroup.DAL.DTOs.Requests.Admin;
using BakerGroup.DAL.DTOs.Requests;
using BakerGroup.DAL.DTOs.Responses.Admin;
using BakerGroup.DAL.DTOs.Responses.User;
using BakerGroup.DAL.DTOs.Responses;
using BakerGroup.DAL.Models;
using BakerGroup.DAL.Repositories.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Http;

#nullable disable

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

    public async Task<PaginatedResponse<UserProductResponse>> GetProductsForUserAsync(ProductQueryRequest query, string language)
    {
        var (products, totalCount, pageNumber, pageSize) = await GetFilteredAndPagedProductsAsync(query, language, includeInactive: false);
        var mappedProducts = products.Select(p => MapUserProduct(p, language)).ToList();
        return new PaginatedResponse<UserProductResponse>(mappedProducts, pageNumber, pageSize, totalCount);
    }

    public async Task<PaginatedResponse<AdminProductResponse>> GetProductsForAdminAsync(ProductQueryRequest query, string language)
    {
        var (products, totalCount, pageNumber, pageSize) = await GetFilteredAndPagedProductsAsync(query, language, includeInactive: true);
        var mappedProducts = products.Select(p => MapAdminProduct(p, language)).ToList();
        return new PaginatedResponse<AdminProductResponse>(mappedProducts, pageNumber, pageSize, totalCount);
    }

    private async Task<(List<Product> Products, int TotalCount, int PageNumber, int PageSize)> GetFilteredAndPagedProductsAsync(ProductQueryRequest query, string language, bool includeInactive)
    {
        query.Validate();

        var products = await _productRepository.GetAllAsync(
            includeInactive ? null : p => p.Status == Status.Active,
            includeProperties: "SubImages,Reviews.User"
        );

        IEnumerable<Product> filteredProducts = products;

        if (query.MinPrice.HasValue)
            filteredProducts = filteredProducts.Where(p => p.Price >= query.MinPrice.Value);
        if (query.MaxPrice.HasValue)
            filteredProducts = filteredProducts.Where(p => p.Price <= query.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            filteredProducts = filteredProducts.Where(p =>
                (p.Name ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (p.NameAr ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (p.Description ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (p.DescriptionAr ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var sortedProducts = ApplySorting(filteredProducts, query, language).ToList();

        var totalCount = sortedProducts.Count;
        var pageNumber = query.PageNumber;
        var pageSize = query.PageSize;
        var pagedProducts = sortedProducts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (pagedProducts, totalCount, pageNumber, pageSize);
    }

    private static IEnumerable<Product> ApplySorting(IEnumerable<Product> products, ProductQueryRequest query, string language)
    {
        var sortBy = query.SortBy?.Trim().ToLowerInvariant();
        var ascending = query.Ascending ?? true;

        return sortBy switch
        {
            "price" => ascending ? products.OrderBy(p => p.Price) : products.OrderByDescending(p => p.Price),
            "rate" => ascending ? products.OrderBy(p => p.Rate) : products.OrderByDescending(p => p.Rate),
            "name" => ascending
                ? products.OrderBy(p => IsArabic(language) ? p.NameAr ?? string.Empty : p.Name ?? string.Empty)
                : products.OrderByDescending(p => IsArabic(language) ? p.NameAr ?? string.Empty : p.Name ?? string.Empty),
            "createdat" => ascending ? products.OrderBy(p => p.CreatedAt) : products.OrderByDescending(p => p.CreatedAt),
            "updatedat" => ascending ? products.OrderBy(p => p.UpdatedAt) : products.OrderByDescending(p => p.UpdatedAt),
            "quantity" => ascending ? products.OrderBy(p => p.Quantity) : products.OrderByDescending(p => p.Quantity),
            _ => products.OrderByDescending(p => p.CreatedAt)
        };
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
                product.SubImages = (product.SubImages ?? new List<ProductImage>())
                    .Where(si => !string.IsNullOrWhiteSpace(si.ImageName))
                    .ToList();

        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        // Make discount optional: default to 0 if not provided
        product.Discount = ResolveDiscount(request.Discount);

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

        // Manually map only the updateable fields to avoid Mapster clearing collections or Status
        if (!string.IsNullOrWhiteSpace(request.Name))
            product.Name = request.Name;
        if (!string.IsNullOrWhiteSpace(request.NameAr))
            product.NameAr = request.NameAr;
        if (!string.IsNullOrWhiteSpace(request.Description))
            product.Description = request.Description;
        if (!string.IsNullOrWhiteSpace(request.DescriptionAr))
            product.DescriptionAr = request.DescriptionAr;
        if (request.Price.HasValue)
            product.Price = request.Price.Value;
        if (request.Quantity.HasValue)
            product.Quantity = request.Quantity.Value;
        if (!string.IsNullOrWhiteSpace(request.CategoryId))
            product.CategoryId = request.CategoryId;
        if (request.Status.HasValue)
            product.Status = request.Status.Value;
        if (request.Discount.HasValue)
            product.Discount = request.Discount.Value;

        product.UpdatedAt = DateTime.UtcNow;

        // Handle MainImage separately without clearing existing collection
        if (request.MainImage != null)
        {
            if (!string.IsNullOrEmpty(product.MainImage))
            {
                var oldFileName = Path.GetFileName(product.MainImage);
                _fileService.DeleteFile(oldFileName, "products/main");
            }
            product.MainImage = await _fileService.UploadFileAsync(request.MainImage, "products/main");
        }

        // Only add new SubImages, don't modify existing ones
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

        // Don't call Update() - entity is already tracked, just SaveAsync()
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

    public async Task<bool> UpdateMainImageAsync(string productId, IFormFile? mainImage)
    {
        var product = (await _productRepository.GetAllAsync(p => p.Id == productId)).FirstOrDefault();
        if (product == null || mainImage == null) return false;

        // Delete old main image if it exists
        if (!string.IsNullOrEmpty(product.MainImage))
        {
            var oldFileName = Path.GetFileName(product.MainImage);
            _fileService.DeleteFile(oldFileName, "products/main");
        }

        // Upload new main image
        product.MainImage = await _fileService.UploadFileAsync(mainImage, "products/main");
        product.UpdatedAt = DateTime.UtcNow;

        return await _productRepository.SaveAsync();
    }

    public async Task<bool> DeleteMainImageAsync(string productId)
    {
        var product = (await _productRepository.GetAllAsync(p => p.Id == productId)).FirstOrDefault();
        if (product == null || string.IsNullOrEmpty(product.MainImage)) return false;

        var oldFileName = Path.GetFileName(product.MainImage);
        _fileService.DeleteFile(oldFileName, "products/main");

        product.MainImage = string.Empty;
        product.UpdatedAt = DateTime.UtcNow;

        return await _productRepository.SaveAsync();
    }

    public async Task<bool> AddSubImagesAsync(string productId, IFormFileCollection subImages)
    {
        var product = (await _productRepository.GetAllAsync(p => p.Id == productId, includeProperties: "SubImages")).FirstOrDefault();
        if (product == null || subImages == null || subImages.Count == 0) return false;

        var subImagePaths = await _fileService.UploadFilesAsync(subImages, "products/sub");
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

        product.UpdatedAt = DateTime.UtcNow;
        return await _productRepository.SaveAsync();
    }

    public async Task<bool> DeleteSubImageAsync(string productId, string subImageId)
    {
        Product? product = await _productRepository.GetByIdAsync(productId, includeProperties: "SubImages");
        if (product == null) return false;

        var subImage = product.SubImages.FirstOrDefault(si => si.Id == subImageId);
        if (subImage == null) return false;

        // Delete file from storage
        if (!string.IsNullOrEmpty(subImage.ImageName))
        {
            var fileName = Path.GetFileName(subImage.ImageName);
            _fileService.DeleteFile(fileName, "products/sub");
        }

        // Remove from database
        product.SubImages.Remove(subImage);
        product.UpdatedAt = DateTime.UtcNow;

        return await _productRepository.SaveAsync();
    }

    private static bool IsArabic(string language) =>
        !string.IsNullOrWhiteSpace(language) && language.StartsWith(ArabicLanguagePrefix, StringComparison.OrdinalIgnoreCase);

    private static decimal ResolveDiscount(decimal? discount) => discount ?? 0m;

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
