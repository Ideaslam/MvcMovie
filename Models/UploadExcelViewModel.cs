using System.ComponentModel.DataAnnotations.Schema;
using MvcMovie.Entities;

namespace MvcMovie.Models;

public class UploadExcelViewModel
{
public IFormFile ExcelFile { get; set; }
    public List<Product>? Products { get; set; }
}
