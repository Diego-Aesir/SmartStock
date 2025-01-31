using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SmartStock.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewData["UserId"] = userId;
            return View();
        }
    }
}
