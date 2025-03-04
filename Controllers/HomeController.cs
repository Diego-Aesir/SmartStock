using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartStock.Models;
using SmartStock.Services.DatabaseServices;

namespace SmartStock.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductDbService _productDbService;
        private readonly ProductCategoryDbService _categoryDbService;


        public HomeController(ProductDbService productDbService, ProductCategoryDbService productCategoryDbService)
        {
            _productDbService = productDbService;
            _categoryDbService = productCategoryDbService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewData["UserId"] = userId;

            try
            {
                var productList = await _productDbService.GetAllProductsByRecentAddition();
                IEnumerable<ProductCategory> categories = await _categoryDbService.GetAllCategories();
                ViewBag.ProductCategories = categories;
                ViewBag.Categories = new SelectList(categories, "Id", "Title");
                return View(productList);
            }
            catch (Exception)
            {
                throw new Exception("Wasn't possible to get products, try again later");
            }
        }
    }
}
