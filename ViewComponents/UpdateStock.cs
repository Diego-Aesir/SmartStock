using Microsoft.AspNetCore.Mvc;

namespace SmartStock.ViewComponents
{
    public class UpdateStock : ViewComponent
    {
        public IViewComponentResult Invoke(int stockId)
        {
            return View(stockId);
        }
    }
}
