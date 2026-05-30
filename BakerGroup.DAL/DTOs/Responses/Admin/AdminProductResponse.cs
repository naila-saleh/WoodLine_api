using BakerGroup.DAL.Models;

namespace BakerGroup.DAL.DTOs.Responses.Admin;

public class AdminProductResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public int Quantity { get; set; }
    public string MainImage { get; set; } = string.Empty;
    public double Rate { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public Status Status { get; set; }
    public List<string> SubImages { get; set; } = new();
    public List<AdminReviewResponse> Reviews { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
