using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MvcMovie.Contracts;
using MvcMovie.Data;
using MvcMovie.Entities;
using MvcMovie.Helpers;
using MvcMovie.Models;
using MvcMovie.Services;
using OfficeOpenXml;
using Serilog;


namespace MvcMovie.Controllers;

public class ProductController : Controller
{
    private readonly ILogger<ProductController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IProductService _productService;
    private readonly IPusher<Product> _pusher;
    IDbContextFactory<ApplicationDbContext> _dbContextFactory;



    public ProductController(ILogger<ProductController> logger,
        ApplicationDbContext context,
        IProductService productService,
        IPusher<Product> pusher,
        IDbContextFactory<ApplicationDbContext> dbContextFactory

        )
    {
        _context = context;
        _logger = logger;
        _productService = productService;
        _pusher = pusher;
        _dbContextFactory = dbContextFactory;


    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var pageSize = 5;
        var query = _context.Products.AsQueryable();

        var productsQuery = _context.Products.AsQueryable(); // or from service

        var paginatedProducts = await PaginatedList<Product>.CreateAsync(productsQuery, page, pageSize);

        return View(paginatedProducts);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        return View(product);
    }

    public IActionResult Upload() => View();

    [HttpPost]
    public async Task<IActionResult> Upload(UploadExcelViewModel model)
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


            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ExcelFile", "Error processing the Excel file: " + ex.Message);
            }
        }
        else
        {
            ModelState.AddModelError("ExcelFile", "Please upload a valid Excel file.");
        }

        return PartialView(new UploadExcelViewModel { Products = products });
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



    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(ProductViewModel model)
    {
        if (ModelState.IsValid)
        {
            string uniqueFileName = null;
            string filePath = null;
            if (model.ImageFile != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory() + "/wwwroot/uploads");
                Directory.CreateDirectory(uploadsFolder);
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

            }

            var product = new Product
            {
                Name = model.Name,
                Price = model.Price,
                Image = uniqueFileName ?? "",
            };
            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);
        var model = new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            ImageFile = null
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ProductViewModel model)
    {

        if (ModelState.IsValid)
        {
            string uniqueFileName = null;
            string filePath = null;
            if (model.ImageFile != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory() + "/wwwroot/uploads");
                Directory.CreateDirectory(uploadsFolder);
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }
            }

            var product = await _context.Products.FindAsync(model.Id);
            if (product == null) return NotFound();
            product.Name = model.Name;
            product.Price = model.Price;
            if (uniqueFileName != null)
            {
                product.Image = uniqueFileName;
            }
            else
            {
                product.Image = product.Image;
            }
            _context.Update(product);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
