using Microsoft.AspNetCore.Mvc;
using SmartStock.Services.DatabaseServices;

namespace SmartStock.ViewComponents
{
    public class CategoryComponent : ViewComponent
    {
        private readonly ProductCategoryDbService _productCategoryDbService;

        public CategoryComponent(ProductCategoryDbService productCategoryDbService) 
        {
            _productCategoryDbService = productCategoryDbService;
        }

        public IViewComponentResult Invoke()
        {
            try
            {
                var categories = _productCategoryDbService.GetAllCategories().Result; 
                return View(categories);
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
