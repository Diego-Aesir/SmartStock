using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models;
using SmartStock.Services.DatabaseServices;

namespace SmartStock.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ProductCategoryDbService _productCategoryDbService;
        private readonly ProductDbService _productDbService;

        public CategoryController(ProductCategoryDbService productCategoryDbService, ProductDbService productDbService)
        {
            _productCategoryDbService = productCategoryDbService;
            _productDbService = productDbService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employees")]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> CreateCategory(ProductCategory productCategory)
        {
            var referer = Request.Headers["Referer"].ToString();
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
                            return Redirect(referer);
                        }
                    }

                    await _productCategoryDbService.CreateProductCategory(productCategory);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }

            return View(productCategory);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategory(int categoryId)
        {
            var products = await _productDbService.GetAllProductsFromCategoryId(categoryId);
            return View(products);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employees")]
        public IActionResult UpdateCategory()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> UpdateCategory(ProductCategory productCategory)
        {
            var referer = Request.Headers["Referer"].ToString();
            if (ModelState.IsValid)
            {
                try
                {
                    await _productCategoryDbService.UpdateProductCategory(productCategory);

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
