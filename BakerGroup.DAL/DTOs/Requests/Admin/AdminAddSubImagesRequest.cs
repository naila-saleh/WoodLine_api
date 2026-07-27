using Microsoft.AspNetCore.Http;

namespace BakerGroup.DAL.DTOs.Requests.Admin;

public class AdminAddSubImagesRequest
{
    public IFormFileCollection? SubImages { get; set; }
}

