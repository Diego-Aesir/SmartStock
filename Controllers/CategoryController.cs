using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models;
using SmartStock.DTO;
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
        public IActionResult UpdateCategory(int categoryId)
        {
            var category = _productCategoryDbService.GetProductCategoryById(categoryId).Result;
            return View(category);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> UpdateCategory(DTO.Category.UpdateCategory productCategory)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ProductCategory updatedCategory = new()
                    {
                        Id = productCategory.Id,
                        Title = productCategory.Title ?? null,
                        Description = productCategory.Description ?? null
                    };

                    await _productCategoryDbService.UpdateProductCategory(updatedCategory);
                    return RedirectToAction("CreateCategory");
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
                var products = await _productDbService.GetAllProductsFromCategoryId(productCategoryId);

                foreach (var product in products)
                {
                    await _productDbService.DeleteProduct(product.Id);
                }

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
