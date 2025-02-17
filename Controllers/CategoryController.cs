using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models;
using SmartStock.Services.DatabaseServices;

namespace SmartStock.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ProductCategoryDbService _productCategoryDbService;

        public CategoryController(ProductCategoryDbService productCategoryDbService)
        {
            _productCategoryDbService = productCategoryDbService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employees")]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> CreateCategory(ProductCategory productCategory, string? Referer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var allCategories = await _productCategoryDbService.GetAllCategories();

                    foreach (var category in allCategories)
                    {
                        if (category.Title == productCategory.Title && category.Description == productCategory.Description) 
                        {
                            ModelState.AddModelError("", "Category already exists");
                            return Redirect(Referer);
                        }
                    }

                    await _productCategoryDbService.CreateProductCategory(productCategory);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employees")]
        public IActionResult UpdateCategory()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> UpdateCategory(ProductCategory productCategory, string? Referer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _productCategoryDbService.UpdateProductCategory(productCategory);

                    if (!string.IsNullOrEmpty(Referer))
                    {
                        return Redirect(Referer);
                    }

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> DeleteCategory(int productCategoryId)
        {
            try
            {
                await _productCategoryDbService.DeleteProductCategory(productCategoryId);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                throw new Exception("Could not delete this category");
            }
        }
    }
}
