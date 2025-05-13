using ClosedXML.Excel;
using MvcMovie.Contracts;
using MvcMovie.Entities;

namespace MvcMovie.Services;

public class ProductService : IProductService
{
    private readonly IPusher<Product> _pusher;
    public ProductService(IPusher<Product> pusher)
    {
        _pusher = pusher;
    }
    
    // Method that returns the transform function
    public Func<IXLRow, Task<Product>> GetTransformRow()
    {
        return async row =>
        {
            var product = new Product
            {
                Name = row.Cell(1).GetString() ?? "",
                Price = decimal.TryParse(row.Cell(2).GetValue<string>(), out var price) ? price : 0,
                Image = row.Cell(3).GetString() ?? ""
            }; 
            return product;
        };
    }

    
}
