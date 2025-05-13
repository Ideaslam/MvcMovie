using ClosedXML.Excel;
using MvcMovie.Entities;
using MvcMovie.Models;

namespace MvcMovie.Contracts;

public interface IProductService
{
     Task<IEnumerable<Product>> GetAllAsync();
     Task<Product> CreateAsync(ProductViewModel model);
     Task<Product> EditAsync(ProductViewModel model);
     Task<IEnumerable<Product>> Upload(UploadExcelViewModel model);
}
