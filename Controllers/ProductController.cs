using Microsoft.AspNetCore.Mvc;
using SmartStock.Services.DatabaseServices;
using SmartStock.Models;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<IActionResult> RegisterProduct(Products product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var productId = await _productDbService.CreateProduct(product);
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
                return View(product);
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> UpdateProduct(Products updatedProduct)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _productDbService.UpdateProduct(updatedProduct);
                    return RedirectToAction("GetProduct", "Product", new { productId = product.Id });
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


        [HttpPost]
        public async Task<IActionResult> SearchProductByTitle()
        {
            throw new NotImplementedException();
        }
    }
}
