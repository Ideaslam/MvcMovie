using System.Diagnostics;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Contracts;
using MvcMovie.Data;
using MvcMovie.Entities;
using MvcMovie.Helpers;
using MvcMovie.Models;
using MvcMovie.Settings;
using Serilog;

namespace MvcMovie.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    public ProductService(ApplicationDbContext context,
        IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _context = context;
    }


    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product> CreateAsync(ProductViewModel model)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }
        var imageName = string.Empty;

        if (model.ImageFile == null
        || !FileHelpers.IsValidSize(model.ImageFile, FileSettings.MaxFileSizeInMB)
        || !FileHelpers.IsValidType(model.ImageFile, FileSettings.ValidImageExtensions))
        {
            throw new ArgumentException("Invalid file");
        }

        (imageName, var imagePath) = FileHelpers.GetFileUniqueName(model.ImageFile);
        using (var stream = new FileStream(imagePath, FileMode.Create))
        {

            await model.ImageFile.CopyToAsync(stream);
        }

        var product = new Product
        {
            Name = model.Name,
            Price = model.Price,
            Currency = model.Currency,
            Image = imageName
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;

    }


    public async Task<Product> EditAsync(ProductViewModel model)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }
        var imageName = string.Empty;

        if (model.ImageFile == null
        || FileHelpers.IsValidSize(model.ImageFile, FileSettings.MaxFileSizeInMB)
        || FileHelpers.IsValidType(model.ImageFile,FileSettings.ValidImageExtensions))
        {
            throw new ArgumentException("Invalid file");
        }

        (imageName, var imagePath) = FileHelpers.GetFileUniqueName(model.ImageFile);
        using (var stream = new FileStream(imagePath, FileMode.Create))
        {

            await model.ImageFile.CopyToAsync(stream);
        }

        var product = await _context.Products.FindAsync(model.Id);
        if (product == null)
        {
            throw new ArgumentException("Product not found");
        }
        product.Name = model.Name;
        product.Price = model.Price;
        product.Currency = model.Currency;
        product.Image = imageName;
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;

    }


    public async Task<IEnumerable<Product>> Upload(UploadExcelViewModel model)
    {
        var products = new List<Product>();
        if (model.ExcelFile != null && model.ExcelFile.Length > 0)
        {
            try
            {
                using var stream = new MemoryStream();
                await model.ExcelFile.CopyToAsync(stream);
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1); // First worksheet
                var rows = worksheet.RowsUsed().Skip(1); // Skip header
                var rowCount = worksheet.LastRowUsed().RowNumber();

                products = await ParrallelPush(rows);
                await ParrallelChunkPush(rows);
                products = await SequescePush(rows);
                return products;


            }
            catch (Exception ex)
            {
                throw new Exception("Error processing the Excel file", ex);
            }
        }
        else
        {
            throw new ArgumentException("Invalid file");
        }

    }


    private async Task ParrallelChunkPush(IEnumerable<IXLRow> rows)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var rowChunks = rows.Chunk(100); // You need .NET 6+ or use custom chunking logic
        var tasks = rowChunks.Select(chunk => Task.Run(async () =>
        {
            using var scopedContext = _dbContextFactory.CreateDbContext(); // use DI or factory
            var products = chunk.Select(row => new Product
            {
                Name = row.Cell(1).GetString() ?? "",
                Price = decimal.TryParse(row.Cell(2).GetValue<string>(), out var price) ? price : 0,
                Image = row.Cell(3).GetString() ?? ""
            }).ToList();

            await scopedContext.Products.AddRangeAsync(products);
            await scopedContext.SaveChangesAsync();
        }));

        await Task.WhenAll(tasks);

        stopwatch.Stop();
        Log.Information($"ParrallelChunkPush completed in {stopwatch.ElapsedMilliseconds} ms");

    }


    private async Task<List<Product>> ParrallelPush(IEnumerable<IXLRow> rows)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        List<Product> products = rows
            .AsParallel()
            .Select(row => new Product
            {
                Name = row.Cell(1).GetString() ?? "",
                Price = decimal.TryParse(row.Cell(2).GetValue<string>(), out var price) ? price : 0,
                Image = row.Cell(3).GetString() ?? ""
            }).ToList();

        await _context.AddRangeAsync(products);
        await _context.SaveChangesAsync();


        stopwatch.Stop();
        Log.Information($"ParrallelPush completed in {stopwatch.ElapsedMilliseconds} ms");

        return products;
    }

    private async Task<List<Product>> SequescePush(IEnumerable<IXLRow> rows)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var products = new List<Product>();
        foreach (var row in rows)
        {
            var product = new Product
            {
                Name = row.Cell(1).GetString() ?? "",
                Price = decimal.TryParse(row.Cell(2).GetValue<string>(), out var price) ? price : 0,
                Image = row.Cell(3).GetString() ?? ""
            };
            products.Add(product);
        }
        await _context.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        stopwatch.Stop();
        Log.Information($"SequescePush completed in {stopwatch.ElapsedMilliseconds} ms");

        return products;

    }



}
