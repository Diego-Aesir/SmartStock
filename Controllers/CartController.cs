using Microsoft.AspNetCore.Mvc;
using SmartStock.Services.DatabaseServices;
using System.Security.Claims;
using SmartStock.Models;
using Microsoft.AspNetCore.Authorization;

namespace SmartStock.Controllers
{
    public class CartController : Controller
    {
        private readonly UserDbService _userDbService;
        private readonly CartDbService _cartDbService;
        private readonly ProductInCartDbService _productInCartDbService;
        private readonly ProductDbService _productDbService;

        public CartController(CartDbService cartDbService, UserDbService userDbService, ProductInCartDbService productInCartDbService, ProductDbService productDbService)
        {
            _userDbService = userDbService;
            _cartDbService = cartDbService;
            _productInCartDbService = productInCartDbService;
            _productDbService = productDbService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCart(string? error)
        {
            var user = await _userDbService.GetUserById(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var userCart = await _cartDbService.GetCartById(user.CartId);
            var productInCart = await _productInCartDbService.GetAllProductsFromCartId(userCart.Id);

            ViewBag.ProductsInCart = productInCart;

            List<Products> products = new List<Products>();

            foreach (var productCart in productInCart)
            {
                var product = await _productDbService.GetProduct(productCart.ProductId);
                products.Add(product);
            }

            ViewBag.Products = products;

            if(error != null)
            {
                ViewBag.Error = error;
            }

            return View(userCart);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToCart(int productId, float discount)
        {
            var referer = Request.Headers["Referer"].ToString();
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userDbService.GetUserById(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    if (user == null)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    var userCart = await _cartDbService.GetCartById(user.CartId);

                    var product = await _productDbService.GetProduct(productId);
                    if (product.QuantityInStock <= 0)
                    {
                        return RedirectToAction("GetProduct", "Product", new { productId = product.Id });
                    }

                    var productsAlreadyInCart = await _productInCartDbService.GetAllProductsFromCartId(userCart.Id);
                    foreach (var productIsInCart in productsAlreadyInCart)
                    {
                        if (productIsInCart.ProductId == productId)
                        {
                            return RedirectToAction("GetCart", "Cart");
                        }
                    }

                    ProductInCart productInCart = new()
                    {
                        CartId = userCart.Id,
                        ProductId = productId,
                        Quantity = 1,
                    };

                    await _productInCartDbService.CreateProductInCart(productInCart);
                    return Redirect(referer);
                }
                catch (Exception)
                {
                    throw new Exception("Could not add product to cart");
                }
            }
            return RedirectToAction("GetProduct", "Product", new { productId = productId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            try
            {
                await _productInCartDbService.DeleteProductInCart(productId);
                return RedirectToAction("GetCart", "Cart");
            }
            catch (Exception)
            {
                throw new Exception("Could not remove product to cart");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddAnotherToCart(int productId)
        {
            try
            {
                var productInCart = await _productInCartDbService.GetProductInCartById(productId);
                var product = await _productDbService.GetProduct(productInCart.ProductId);

                if (product.QuantityInStock <= 0)
                {
                    return RedirectToAction("GetProduct", "Product", new { productId = product.Id });
                }

                if (product.QuantityInStock <= productInCart.Quantity)
                {
                    return RedirectToAction("GetCart", "Cart", new {error = $"You already have added all \"{product.Title}\" available on our stocks"});
                }

                await _productInCartDbService.UpdateProductQuantity(productId, productInCart.Quantity + 1);
                return RedirectToAction("GetCart", "Cart");
            }
            catch (Exception)
            {
                throw new Exception("Could not remove product to cart");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveOneFromCart(int productId)
        {
            try
            {
                var productInCart = await _productInCartDbService.GetProductInCartById(productId);
                var product = await _productDbService.GetProduct(productInCart.ProductId);

                if (productInCart.Quantity == 1)
                {
                    return await RemoveFromCart(productId);
                }

                await _productInCartDbService.UpdateProductQuantity(productId, productInCart.Quantity - 1);
                return RedirectToAction("GetCart", "Cart");
            }
            catch (Exception)
            {
                throw new Exception("Could not remove product to cart");
            }

        }

    }
}
