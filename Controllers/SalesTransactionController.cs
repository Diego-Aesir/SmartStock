using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SmartStock.DTO.User;
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
        private readonly ProductSoldDbService _productSoldDbService;

        public SalesTransactionController(
            UserDbService userDbService,
            CartDbService cartDbService,
            ProductInCartDbService productInCartDbService,
            ProductDbService productDbService,
            SalesTransactionDbService salesTransactionDbService,
            ProductSoldDbService productSoldDbService
        ){
            _cartDbService = cartDbService;
            _productInCartDbService = productInCartDbService;
            _userDbService = userDbService;
            _productDbService = productDbService;
            _salesTransactionDbService = salesTransactionDbService;
            _productSoldDbService = productSoldDbService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> FinalizeBuy(int? productId, string? error)
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
            }

            var products = await _productInCartDbService.GetAllProductsFromCartId(userCart.Id);
            ViewBag.ProductInCart = products;

            List<Products> product = new List<Products>();
            foreach (var pr in products)
            {
                product.Add(await _productDbService.GetProduct(pr.ProductId));
            }

            if (error != null)
            {
                ViewBag.Error = error;
            }

            ViewBag.Address = user.CEP;

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

            var productsInCart = await _productInCartDbService.GetAllProductsFromCartId(cartId);

            foreach (var product in productsInCart)
            {
                var productOnStock = await _productDbService.GetProduct(product.ProductId);

                if (productOnStock.QuantityInStock == 0)
                {
                    await _productInCartDbService.DeleteProductInCart(product.Id);
                    return RedirectToAction("FinalizeBuy", new { error = $"\"{productOnStock.Title}\" is sold out. Removing from your cart..." });
                }

                if (product.Quantity <= productOnStock.QuantityInStock)
                {
                    await _productDbService.ChangeProductQuantity(productOnStock.Id, productOnStock.QuantityInStock - product.Quantity);
                }
                else
                {
                    await _productInCartDbService.UpdateProductQuantity(product.Id, productOnStock.QuantityInStock);
                    return RedirectToAction("FinalizeBuy", new { error = $"\"{productOnStock.Title}\" has only {productOnStock.QuantityInStock} available. Updating your quantity..." });
                }
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
                SaleDiscount = discount,
                DeliverCEP = user.CEP
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SoldProducts(int saleId)
        {
            var products = await _productSoldDbService.GetAllProductsSoldByTransactionId(saleId);
            return View(products);
        }
    }
}
