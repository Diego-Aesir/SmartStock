using Microsoft.AspNetCore.Mvc;
using SmartStock.Models;

namespace SmartStock.ViewComponents
{
    public class ProductComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Products product)
        {
            return View(product);
        }
    }
}
