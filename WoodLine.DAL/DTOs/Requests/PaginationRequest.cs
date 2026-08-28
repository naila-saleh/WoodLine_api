namespace WoodLine.DAL.DTOs.Requests;

public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 8;

    public PaginationRequest()
    {
    }

    public PaginationRequest(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public void Validate()
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 1) PageSize = 8;
        if (PageSize > 100) PageSize = 100; // Max page size to prevent abuse
    }
}

