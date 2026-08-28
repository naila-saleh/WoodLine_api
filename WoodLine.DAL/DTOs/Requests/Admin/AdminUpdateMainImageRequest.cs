using Microsoft.AspNetCore.Http;

namespace WoodLine.DAL.DTOs.Requests.Admin;

public class AdminUpdateMainImageRequest
{
    public IFormFile? MainImage { get; set; }
}

