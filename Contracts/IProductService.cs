using ClosedXML.Excel;
using MvcMovie.Entities;

namespace MvcMovie.Contracts;

public interface IProductService
{
     public Func<IXLRow, Task<Product>> GetTransformRow() ;
}
