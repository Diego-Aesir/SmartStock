using Microsoft.AspNetCore.Mvc;
using SmartStock.Models;
using SmartStock.DTO.CategoryDiscount;
using SmartStock.Services.DatabaseServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartStock.Controllers
{
    public class CategoryDiscountController : Controller
    {
        private readonly CategoryDiscountDbService _categoryDiscountDbService;
        private readonly ProductCategoryDbService _productCategoryDbService;

        public CategoryDiscountController(CategoryDiscountDbService categoryDiscountDbService, ProductCategoryDbService productCategoryDbService)
        {
            _categoryDiscountDbService = categoryDiscountDbService;
            _productCategoryDbService = productCategoryDbService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employees")]
        public IActionResult CreateDiscount()
        {
            IEnumerable<ProductCategory> categories = _productCategoryDbService.GetAllCategories().Result;
            ViewBag.ProductCategoryId = new SelectList(categories, "Id", "Title");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> CreateDiscount(CategoryDiscount categoryDiscount)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var created = await _categoryDiscountDbService.CreateDiscountCategory(categoryDiscount);
                    if (categoryDiscount.IsApplied)
                    {
                        await _categoryDiscountDbService.ManuallyApplyDiscount(created);
                    }
                    return RedirectToAction("CreateDiscount");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            IEnumerable<ProductCategory> categories = _productCategoryDbService.GetAllCategories().Result;
            ViewBag.ProductCategoryId = new SelectList(categories, "Id", "Title");
            return View(categoryDiscount);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> UpdateDiscount(int discountId)
        {

            var discountCategory = await _categoryDiscountDbService.GetCategoryDiscountById(discountId);

            UpdateCategoryDiscount discount = new()
            {
                Id = discountId,
                Title = discountCategory.Title,
                Discount = discountCategory.Discount,
                ProductCategoryId = discountCategory.ProductCategoryId,
                DiscountStartDate = discountCategory.DiscountStartDate,
                DiscountEndDate = discountCategory.DiscountEndDate,
            };

            IEnumerable<ProductCategory> categories = await _productCategoryDbService.GetAllCategories();
            ViewBag.ProductCategoryId = new SelectList(categories, "Id", "Title");

            return View(discount);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> UpdateDiscount(UpdateCategoryDiscount updatedDiscount)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var categoryUpdated = await _categoryDiscountDbService.UpdateCategoryDiscount(updatedDiscount.Id, updatedDiscount);
                    return RedirectToAction("CreateDiscount");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            IEnumerable<ProductCategory> categories = await _productCategoryDbService.GetAllCategories();
            ViewBag.ProductCategoryId = new SelectList(categories, "Id", "Title");

            return View(updatedDiscount);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> ApplyDiscount(int discountId)
        {
            var referer = Request.Headers["Referer"].ToString();
            try
            {
                await _categoryDiscountDbService.ManuallyApplyDiscount(discountId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return Redirect(referer);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> DisableDiscount(int discountId)
        {
            var referer = Request.Headers["Referer"].ToString();
            try
            {
                await _categoryDiscountDbService.ManuallyDisableDiscount(discountId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return Redirect(referer);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> DeleteDiscount(int discountId)
        {
            var referer = Request.Headers["Referer"].ToString();
            if (ModelState.IsValid)
            {
                try
                {
                    var done = await _categoryDiscountDbService.DeleteCategoryDiscount(discountId);
                    return Redirect(referer);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View();
        }

    }
}
