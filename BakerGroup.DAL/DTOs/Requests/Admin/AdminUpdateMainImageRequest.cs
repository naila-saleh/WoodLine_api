using Microsoft.AspNetCore.Http;

namespace BakerGroup.DAL.DTOs.Requests.Admin;

public class AdminUpdateMainImageRequest
{
    public IFormFile? MainImage { get; set; }
}

