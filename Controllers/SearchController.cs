using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models;
using SmartStock.Services.DatabaseServices;

namespace SmartStock.Controllers
{
    public class SearchController : Controller
    {
        private readonly ProductDbService _productDbService;

        public SearchController(ProductDbService productDbService)
        {
            _productDbService = productDbService;
        }

        [HttpPost]
        public async Task<IActionResult> GetSearch(string search, int categoryId)
        {
            IEnumerable<Products> products = await _productDbService.GetAllProductsFromCategoryId(categoryId);
            string cleanedSearch = Regex.Replace(search, @"\s+", " ").Trim().ToLower();

            var filteredProducts = products.Where(p =>
            {
                string cleanedTitle = Regex.Replace(p.Title, @"\s+", " ").Trim().ToLower();
                return cleanedTitle.Contains(cleanedSearch);
            }).ToList();

            if (!filteredProducts.Any())
            {
                ViewBag.AltProduct = await _productDbService.FindProductByTitle(search);
            }

            return View(filteredProducts);
        }
    }
}
