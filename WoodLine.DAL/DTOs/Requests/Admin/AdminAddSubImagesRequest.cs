using Microsoft.AspNetCore.Http;

namespace WoodLine.DAL.DTOs.Requests.Admin;

public class AdminAddSubImagesRequest
{
    public IFormFileCollection? SubImages { get; set; }
}

