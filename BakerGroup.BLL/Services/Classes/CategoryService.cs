using System;
using System.Linq;
using BakerGroup.BLL.Services.Interfaces;
using BakerGroup.DAL.DTOs.Requests.Admin;
using BakerGroup.DAL.DTOs.Responses.Admin;
using BakerGroup.DAL.DTOs.Responses.User;
using BakerGroup.DAL.Models;
using BakerGroup.DAL.Repositories.Interfaces;
using Mapster;

namespace BakerGroup.BLL.Services.Classes;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IFileService _fileService;

    public CategoryService(ICategoryRepository categoryRepository, IFileService fileService)
    {
        _categoryRepository = categoryRepository;
        _fileService = fileService;
    }

    public async Task<IEnumerable<UserCategoryResponse>> GetAllCategoriesForUserAsync()
    {
        var categories = await _categoryRepository.GetAllAsync(c => c.Status == Status.Active);
        return categories.Adapt<IEnumerable<UserCategoryResponse>>();
    }

    public async Task<IEnumerable<AdminCategoryResponse>> GetAllCategoriesForAdminAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Adapt<IEnumerable<AdminCategoryResponse>>();
    }

    public async Task<UserCategoryResponse?> GetCategoryByIdForUserAsync(string id)
    {
        var category = await _categoryRepository.GetByIdAsync(id, includeProperties: "Products");
        if (category == null || category.Status != Status.Active) return null;
        return category.Adapt<UserCategoryResponse>();
    }

    public async Task<AdminCategoryResponse?> GetCategoryByIdForAdminAsync(string id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category?.Adapt<AdminCategoryResponse>();
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
}
