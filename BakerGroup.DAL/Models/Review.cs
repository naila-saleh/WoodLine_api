namespace BakerGroup.DAL.Models;

public class Review : BaseModel
{
    public int Rate { get; set; }
    public string Comment { get; set; }
    public string ProductId { get; set; }
    public Product Product { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}