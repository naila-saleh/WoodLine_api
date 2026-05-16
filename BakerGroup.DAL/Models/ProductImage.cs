namespace BakerGroup.DAL.Models;

public class ProductImage : BaseModel
{
    public string ImageName { get; set; }
    public string ProductId { get; set; }
    public Product Product { get; set; }
}