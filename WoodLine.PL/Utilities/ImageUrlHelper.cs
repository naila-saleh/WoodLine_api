using WoodLine.DAL.DTOs.Responses.Admin;
using WoodLine.DAL.DTOs.Responses.User;

namespace WoodLine.PL.Utilities;

public static class ImageUrlHelper
{
    public static void NormalizeCategoryImages(this IEnumerable<UserCategoryResponse> categories, string baseUrl)
    {
        foreach (var category in categories)
        {
            NormalizeCategoryImages(category, baseUrl);
        }
    }

    public static void NormalizeCategoryImages(this IEnumerable<AdminCategoryResponse> categories, string baseUrl)
    {
        foreach (var category in categories)
        {
            NormalizeCategoryImages(category, baseUrl);
        }
    }

    public static void NormalizeCategoryImages(this UserCategoryResponse category, string baseUrl)
    {
        category.Image = ToAbsoluteUrl(baseUrl, category.Image);

        foreach (var product in category.Products)
        {
            NormalizeProductImages(product, baseUrl);
        }
    }

    public static void NormalizeCategoryImages(this AdminCategoryResponse category, string baseUrl)
    {
        category.Image = ToAbsoluteUrl(baseUrl, category.Image);

        foreach (var product in category.Products)
        {
            NormalizeProductImages(product, baseUrl);
        }
    }

    public static void NormalizeProductImages(this IEnumerable<UserProductResponse> products, string baseUrl)
    {
        foreach (var product in products)
        {
            NormalizeProductImages(product, baseUrl);
        }
    }

    public static void NormalizeProductImages(this IEnumerable<AdminProductResponse> products, string baseUrl)
    {
        foreach (var product in products)
        {
            NormalizeProductImages(product, baseUrl);
        }
    }

    public static void NormalizeProductImages(this UserProductResponse product, string baseUrl)
    {
        product.MainImage = ToAbsoluteUrl(baseUrl, product.MainImage);

        for (var i = 0; i < product.SubImages.Count; i++)
        {
            product.SubImages[i] = ToAbsoluteUrl(baseUrl, product.SubImages[i]);
        }
    }

    public static void NormalizeProductImages(this AdminProductResponse product, string baseUrl)
    {
        product.MainImage = ToAbsoluteUrl(baseUrl, product.MainImage);

        for (var i = 0; i < product.SubImages.Count; i++)
        {
            product.SubImages[i] = ToAbsoluteUrl(baseUrl, product.SubImages[i]);
        }
    }

    private static string ToAbsoluteUrl(string baseUrl, string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return imagePath ?? string.Empty;

        return $"{baseUrl.TrimEnd('/')}/{imagePath.TrimStart('/')}";
    }
}
