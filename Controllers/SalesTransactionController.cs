using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models;
using SmartStock.Services.DatabaseServices;

namespace SmartStock.Controllers
{
    public class SalesTransactionController : Controller
    {
        private readonly UserDbService _userDbService;
        private readonly CartDbService _cartDbService;
        private readonly ProductInCartDbService _productInCartDbService;
        private readonly ProductDbService _productDbService;
        private readonly SalesTransactionDbService _salesTransactionDbService;

        public SalesTransactionController(UserDbService userDbService, CartDbService cartDbService,
                                            ProductInCartDbService productInCartDbService,
                                            ProductDbService productDbService, SalesTransactionDbService salesTransactionDbService)
        {
            _cartDbService = cartDbService;
            _productInCartDbService = productInCartDbService;
            _userDbService = userDbService;
            _productDbService = productDbService;
            _salesTransactionDbService = salesTransactionDbService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> FinalizeBuy(int? productId)
        {
            var user = await _userDbService.GetUserById(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var userCart = await _cartDbService.GetCartById(user.CartId);
            ViewBag.CartId = userCart.Id;

            if (productId != null)
            {
                var productOnStock = await _productDbService.GetProduct(productId.Value);

                if (productOnStock.QuantityInStock <= 0)
                {
                    return RedirectToAction("GetProduct", "Product", new { productId = productOnStock.Id });
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
                    ProductId = productId.Value,
                    Quantity = 1
                };

                await _productInCartDbService.CreateProductInCart(productInCart);
                await _productDbService.ChangeProductQuantity(productId.Value, productOnStock.QuantityInStock - 1);
            }

            var products = await _productInCartDbService.GetAllProductsFromCartId(userCart.Id);
            ViewBag.ProductInCart = products;

            List<Products> product = new List<Products>();
            foreach (var pr in products) {
                product.Add(await _productDbService.GetProduct(pr.ProductId));
            }
            return View(product);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> FinalizeBuy(int cartId, int paymentOpt, decimal discount)
        {
            var user = await _userDbService.GetUserById(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            switch (paymentOpt)
            {
                case 0:
                    ViewBag.PaymentMethod = "Cash";
                    break;
                case 1:
                    ViewBag.PaymentMethod = "CreditCard";
                    break;
                case 2:
                    ViewBag.PaymentMethod = "SmartStoreCredit";
                    break;
            }

            SalesTransaction sale = new()
            {
                ClientId = user.Id,
                PaymentMethod = ViewBag.PaymentMethod,
                SaleDiscount = discount
            };

            int saleId = await _salesTransactionDbService.CreateTransaction(sale);
            await _salesTransactionDbService.CompleteTransaction(saleId, cartId);
            return RedirectToAction("Thanks");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Thanks()
        {
            return View();
        }
    }
}
