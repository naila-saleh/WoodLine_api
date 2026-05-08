namespace BakerGroup.DAL.Models;

public class ProductImage : BaseModel
{
    public string ImageName { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
}