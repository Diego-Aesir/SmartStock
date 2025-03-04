using Microsoft.AspNetCore.Mvc;
using SmartStock.Services.DatabaseServices;

namespace SmartStock.ViewComponents
{
    public class DiscountComponent : ViewComponent
    {
        private readonly CategoryDiscountDbService _discountDbService;

        public DiscountComponent(CategoryDiscountDbService categoryDiscountDbService) 
        {
            _discountDbService = categoryDiscountDbService;
        }

        public IViewComponentResult Invoke()
        {
            try
            {
                var discount = _discountDbService.GetAllCategoryDiscounts().Result;
                return View(discount);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
