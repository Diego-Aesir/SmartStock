using Microsoft.AspNetCore.Mvc;
using SmartStock.Services.DatabaseServices;
using SmartStock.Models;
using Microsoft.AspNetCore.Authorization;
using SmartStock.DTO.Product;

namespace SmartStock.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductDbService _productDbService;
        private readonly StockDbService _stockDbService;
        private readonly ProductCategoryDbService _categoryDbService;

        public ProductController(ProductDbService productDbService, StockDbService stockDbService, ProductCategoryDbService productCategoryDbService)
        {
            _productDbService = productDbService;
            _stockDbService = stockDbService;
            _categoryDbService = productCategoryDbService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProduct(int productId)
        {
            try
            {
                var product = await _productDbService.GetProduct(productId);
                return View(product);
            }
            catch (Exception)
            {
                throw new Exception("Could not find this product");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> RegisterProduct()
        {
            ViewBag.Stocks = await _stockDbService.GetAllStockAsync();
            ViewBag.Categories = await _categoryDbService.GetAllCategories();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> RegisterProduct(ProductIForm product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var newProduct = await product.TurnIFormIntoClass();
                    var productId = await _productDbService.CreateProduct(newProduct);
                    return RedirectToAction("GetProduct", "Product", new { productId = productId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            ViewBag.Stocks = await _stockDbService.GetAllStockAsync();
            ViewBag.Categories = await _categoryDbService.GetAllCategories();

            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> UpdateProduct(int productId)
        {
            try
            {
                ViewBag.Stocks = await _stockDbService.GetAllStockAsync();
                ViewBag.Categories = await _categoryDbService.GetAllCategories();

                var product = await _productDbService.GetProduct(productId);
                ProductIForm productIForm = new()
                {
                    Id = product.Id,
                    Title = product.Title,
                    Description = product.Description ?? "",
                    CategoryId = product.CategoryId,
                    StockId = product.StockId,
                    Price = product.Price,
                    QuantityInStock = product.QuantityInStock,
                    AddedTime = product.AddedTime,
                    Discount = product.Discount
                };

                return View(productIForm);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> UpdateProduct(ProductIForm updatedProduct)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (updatedProduct.Photo != null)
                    {
                        var productPhotoUpdate = await _productDbService.GetProduct(updatedProduct.Id.Value);
                        if (!string.IsNullOrEmpty(productPhotoUpdate.Photo))
                        {
                            updatedProduct.RemovePhoto(productPhotoUpdate.Photo);
                        }
                    }
                    var product = await updatedProduct.TurnIFormIntoClass();
                    product.Id = updatedProduct.Id.Value;

                    var productUpdate = await _productDbService.UpdateProduct(product);
                    return RedirectToAction("GetProduct", "Product", new { productId = productUpdate.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            ViewBag.Stocks = await _stockDbService.GetAllStockAsync();
            ViewBag.Categories = await _categoryDbService.GetAllCategories();

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            try
            {
                await _productDbService.DeleteProduct(productId);
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
