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

        public SalesTransactionController(UserDbService userDbService, CartDbService cartDbService, ProductInCartDbService productInCartDbService, ProductDbService productDbService, SalesTransactionDbService salesTransactionDbService)
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

            if (productId != null)
            {
                var product = await _productDbService.GetProduct(productId.Value);

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
                    ProductId = productId.Value,
                    Quantity = 1
                };

                await _productInCartDbService.CreateProductInCart(productInCart);
                await _productDbService.ChangeProductQuantity(productId.Value, product.QuantityInStock - 1);
            }

            var products = await _productInCartDbService.GetProductInCartById(userCart.Id);
            return View(products);
        }

        [HttpPost]
        [Authorize]
        public async Task FinalizeBuy(int cartId, int paymentOpt)
        {
            var user = await _userDbService.GetUserById(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (user == null)
            {
                return;
            }

            switch (paymentOpt)
            {
                case 0:
                    ViewBag.Paid = "Cash";
                    break;
                case 1:
                    ViewBag.Paid = "CreditCard";
                    break;
                case 2:
                    ViewBag.Paid = "SmartStoreCredit";
                    break;
            }


            /*

            SalesTransaction sale = new()
            {
                ClientId = user.Id,


            }
            await _salesTransactionDbService.CreateTransaction();
            */

            //Salvar nova Sale no Bd
        }
    }
}
