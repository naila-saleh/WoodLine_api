namespace WoodLine.DAL.DTOs.Responses.User;

public class UserReviewResponse
{
    public string Id { get; set; } = string.Empty;
    public int Rate { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
