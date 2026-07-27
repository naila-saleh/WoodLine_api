namespace BakerGroup.DAL.DTOs.Requests.User;

public class ProductFilterRequest
{
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    
    public SortType? PriceSort { get; set; }
    
    public SortType? RateSort { get; set; }
    
    public SortType? NameSort { get; set; }
    
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 8;

    public void Validate()
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 1) PageSize = 8;
        if (PageSize > 1000) PageSize = 1000;

        if (MinPrice.HasValue && MinPrice.Value < 0) MinPrice = 0;
        if (MaxPrice.HasValue && MaxPrice.Value < 0) MaxPrice = 0;

        // Swap if min > max
        if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice > MaxPrice)
        {
            var temp = MinPrice;
            MinPrice = MaxPrice;
            MaxPrice = temp;
        }
    }
}

public enum SortType
{
    LowToHigh,
    HighToLow
}
