using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Services.DatabaseServices;

namespace SmartStock.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductDbService _productDbService;

        public HomeController(ProductDbService productDbService)
        {
            _productDbService = productDbService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewData["UserId"] = userId;

            try
            {
                var productList = await _productDbService.GetAllProductsByRecentAddition();
                return View(productList);
            }
            catch (Exception)
            {
                throw new Exception("Wasn't possible to get products, try again later");
            }
        }
    }
}
