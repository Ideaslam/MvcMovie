using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Rendering;
using MvcMovie.Entities;

namespace MvcMovie.Contracts;

public interface ICurrencyService
{
    IEnumerable<SelectListItem> GetSelectList();
}
