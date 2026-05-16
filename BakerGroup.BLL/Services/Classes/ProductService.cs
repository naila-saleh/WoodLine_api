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
    private readonly IProductRepository _productRepository;
    private readonly IFileService _fileService;

    public ProductService(IProductRepository productRepository, IFileService fileService)
    {
        _productRepository = productRepository;
        _fileService = fileService;
    }

    public async Task<IEnumerable<UserProductResponse>> GetAllProductsForUserAsync()
    {
        var products = await _productRepository.GetAllAsync(p => p.Status == Status.Active, includeProperties: "SubImages");
        return products.Select(p =>
        {
            var res = p.Adapt<UserProductResponse>();
            res.SubImages = p.SubImages.Select(si => si.ImageName).ToList();
            return res;
        });
    }

    public async Task<IEnumerable<AdminProductResponse>> GetAllProductsForAdminAsync()
    {
        var products = await _productRepository.GetAllAsync(includeProperties: "SubImages");
        return products.Select(p =>
        {
            var res = p.Adapt<AdminProductResponse>();
            res.SubImages = p.SubImages.Select(si => si.ImageName).ToList();
            return res;
        });
    }

    public async Task<UserProductResponse?> GetProductByIdForUserAsync(string id)
    {
        var product = await _productRepository.GetByIdAsync(id, includeProperties: "SubImages");
        if (product == null || product.Status != Status.Active) return null;

        var res = product.Adapt<UserProductResponse>();
        res.SubImages = product.SubImages.Select(si => si.ImageName).ToList();
        return res;
    }

    public async Task<AdminProductResponse?> GetProductByIdForAdminAsync(string id)
    {
        var product = await _productRepository.GetByIdAsync(id, includeProperties: "SubImages");
        if (product == null) return null;

        var res = product.Adapt<AdminProductResponse>();
        res.SubImages = product.SubImages.Select(si => si.ImageName).ToList();
        return res;
    }

    public async Task<AdminProductResponse> CreateProductAsync(AdminCreateProductRequest request)
    {
        var product = request.Adapt<Product>();
        // Ensure collections aren't overwritten to null by Mapster
        product.SubImages = product.SubImages ?? new List<ProductImage>();

        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        // Make discount optional: default to 0 if not provided
        product.Discount = request.Discount ?? 0m;

        if (request.MainImage != null)
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
        result.SubImages = (product.SubImages ?? new List<ProductImage>()).Select(si => si.ImageName).ToList();
        return result;
    }

    public async Task<bool> UpdateProductAsync(string id, AdminUpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id, includeProperties: "SubImages");
        if (product == null) return false;

        request.Adapt(product);
        // Ensure collections aren't null after mapping
        product.SubImages = product.SubImages ?? new List<ProductImage>();

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
}
