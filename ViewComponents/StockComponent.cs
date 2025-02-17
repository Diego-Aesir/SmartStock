using Microsoft.AspNetCore.Mvc;
using SmartStock.Services.DatabaseServices;

namespace SmartStock.ViewComponents
{
    public class StockComponent : ViewComponent
    {
        private readonly StockDbService _stockDbService;
        public StockComponent(StockDbService stockDbService) 
        {
            _stockDbService = stockDbService;
        }

        public IViewComponentResult Invoke()
        {
            var stocks = _stockDbService.GetAllStockAsync().Result;

            return View(stocks);
        }
    }
}
