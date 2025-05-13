using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Contracts;
using MvcMovie.Data;
using MvcMovie.Entities;

namespace MvcMovie.Services;

public class CurrencyService : ICurrencyService
{
    private readonly IEnumerable<SelectListItem> currencies = new List<SelectListItem>
            {
                new SelectListItem { Value = "SAR", Text = "SAR" },
                new SelectListItem { Value = "USD", Text = "USD" },
                new SelectListItem { Value = "EGP", Text = "EGP" }
            };
    private readonly ApplicationDbContext _context;
    public CurrencyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public  IEnumerable<SelectListItem> GetSelectList()
    {
       return currencies;
    }
}
