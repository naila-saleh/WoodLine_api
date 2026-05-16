using BakerGroup.DAL.Models;

namespace BakerGroup.DAL.DTOs.Responses.Admin;

public class AdminCategoryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
