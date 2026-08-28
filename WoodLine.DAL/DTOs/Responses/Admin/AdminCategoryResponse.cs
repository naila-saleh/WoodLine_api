using WoodLine.DAL.Models;

namespace WoodLine.DAL.DTOs.Responses.Admin;

public class AdminCategoryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AdminProductResponse> Products { get; set; } = new();
}
