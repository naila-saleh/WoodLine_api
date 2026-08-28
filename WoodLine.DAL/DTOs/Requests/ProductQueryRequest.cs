namespace WoodLine.DAL.DTOs.Requests;

public class ProductQueryRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 8;

    public string? SortBy { get; set; }
    public bool? Ascending { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Search { get; set; }

    public void Validate()
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 1) PageSize = 8;
        if (PageSize > 1000) PageSize = 1000;

        if (MinPrice.HasValue && MinPrice.Value < 0) MinPrice = 0;
        if (MaxPrice.HasValue && MaxPrice.Value < 0) MaxPrice = 0;

        if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice > MaxPrice)
        {
            (MinPrice, MaxPrice) = (MaxPrice, MinPrice);
        }

        SortBy = string.IsNullOrWhiteSpace(SortBy) ? null : SortBy.Trim();
        Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();
    }
}

