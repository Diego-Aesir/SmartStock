using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models;
using SmartStock.Services.DatabaseServices;

namespace SmartStock.Controllers
{
    public class StockController : Controller
    {
        private readonly StockDbService _stockDbService;

        public StockController(StockDbService stockDbService)
        {
            _stockDbService = stockDbService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employees")]
        public IActionResult CreateStock()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> CreateStock(Stock stock)
        {
            var referer = Request.Headers["Referer"].ToString();
            if (ModelState.IsValid)
            {
                try
                {
                    var allStocks = await _stockDbService.GetAllStockAsync();

                    foreach (var stockExist in allStocks)
                    {
                        if (stockExist.Name == stock.Name)
                        {
                            ModelState.AddModelError("", "Stock already exists");
                            return Redirect(referer);
                        }
                    }

                    await _stockDbService.CreateStockAsync(stock);

                    if (!string.IsNullOrEmpty(referer))
                    {
                        return Redirect(referer);
                    }

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(stock);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employees")]
        public IActionResult DeleteStock()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> DeleteStock(int stockId)
        {
            try
            {
                await _stockDbService.DeleteStock(stockId);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }
    }
}
