using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MvcMovie.Attributes;
using MvcMovie.Settings;

namespace MvcMovie.Models;

public class ProductViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; }


    public IEnumerable<SelectListItem> Currencies { get; set; } = Enumerable.Empty<SelectListItem>();

  
    [AllowedExtention( FileSettings.ValidImageExtensions)]
    [MaxFileSize(FileSettings.MaxFileSizeInMB)]
    public IFormFile ImageFile { get; set; }  // File upload

    public string ImageName { get; set; }
}
