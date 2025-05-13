using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    private readonly ICurrencyService _currencyService;




    public ProductController(ILogger<ProductController> logger,
        ApplicationDbContext context,
        IProductService productService,
        IPusher<Product> pusher,
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ICurrencyService currencyService


        )
    {
        _context = context;
        _logger = logger;
        _productService = productService;
        _pusher = pusher;
        _dbContextFactory = dbContextFactory;
        _currencyService = currencyService;


    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var pageSize = 5;

        var productsQuery = _context.Products.OrderByDescending(x => x.Id).AsQueryable(); // or from service

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
        var products = (await _productService.Upload(model)).ToList();
        return PartialView(new UploadExcelViewModel { Products = products });
    }


    public IActionResult Create()
    {
        var model = new ProductViewModel
        {
            Currencies = _currencyService.GetSelectList()
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductViewModel model)
    {
        try
        {
            ModelState.Remove(nameof(ProductViewModel.ImageName));
            if (!ModelState.IsValid)
            {
                model.Currencies = _currencyService.GetSelectList();
                return View(model);
            }
            await _productService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Error creating product");
            ModelState.AddModelError(nameof(ProductViewModel.ImageFile), ex.Message);
            model.Currencies = _currencyService.GetSelectList();
            return View(model);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the product.");
            model.Currencies = _currencyService.GetSelectList();
            return View(model);
        }

    }

    public async Task<IActionResult> Edit(int id)
    {

        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        var model = new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Currency = product.Currency,
            Currencies = _currencyService.GetSelectList(),
            ImageName = product.Image
        };
        return View(model);

    }

    [HttpPost]
    public async Task<IActionResult> Edit(ProductViewModel model)
    {
        try
        {
            // Exclude ImageName from validation
            ModelState.Remove(nameof(ProductViewModel.ImageName));

            if (!ModelState.IsValid)
            {
                model.Currencies = _currencyService.GetSelectList();
                return View(model);
            }
            await _productService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the product.");
            model.Currencies = _currencyService.GetSelectList();
            return View(model);
        }

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
        if (product == null) return NotFound();

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
