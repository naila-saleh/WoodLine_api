namespace BakerGroup.DAL.DTOs.Responses.User;

public class UserProductResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public string MainImage { get; set; } = string.Empty;
    public double Rate { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public List<string> SubImages { get; set; } = new();
    public List<UserReviewResponse> Reviews { get; set; } = new();
}
