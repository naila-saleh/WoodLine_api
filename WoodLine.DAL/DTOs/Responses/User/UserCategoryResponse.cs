namespace WoodLine.DAL.DTOs.Responses.User;

public class UserCategoryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public List<UserProductResponse> Products { get; set; } = new();
}
