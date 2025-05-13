using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMovie.Models;

public class ProductViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }

    public decimal Price { get; set; }

    [NotMapped]
    public IFormFile ImageFile { get; set; }  // File upload
}
