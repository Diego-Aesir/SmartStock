﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartStock.DTO.User;
using SmartStock.Models;
using SmartStock.Services.DatabaseServices;

namespace SmartStock.Controllers
{
    public class UserController : Controller
    {
        public readonly UserDbService _userDbService;
        private readonly SignInManager<User> _signInManager;
        private readonly CartDbService _cartDbService;
        private readonly SalesTransactionDbService _salesTransactionDbService;

        public UserController(
            UserDbService userDbService,
            SignInManager<User> signInManager,
            CartDbService cartDbService,
            SalesTransactionDbService salesTransactionDbService
        ){
            _userDbService = userDbService;
            _signInManager = signInManager;
            _cartDbService = cartDbService;
            _salesTransactionDbService = salesTransactionDbService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserRegisterDTO user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int cartId = await _cartDbService.CreateCart();

                    User newUser = new User
                    {
                        UserName = user.UserName,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.Phone,
                        CEP = user.CEP,
                        CartId = cartId
                    };

                    newUser = await _userDbService.CreateUser(newUser, user.Password);
                    await _signInManager.SignInAsync(newUser, isPersistent: false);

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult CreateEmployee()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee(UserRegisterDTO user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int cartId = await _cartDbService.CreateCart();

                    User newUser = new User
                    {
                        UserName = user.UserName,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.Phone,
                        CEP = user.CEP,
                        CartId = cartId
                    };

                    newUser = await _userDbService.CreateEmployee(newUser, user.Password);

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(user);
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginDTO user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var loginUser = await _userDbService.LoginUser(user.Email, user.Password);
                    await _signInManager.SignInAsync(loginUser, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                }
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    ViewData["Error"] = "Your user id could not be retrieved";
                    return View();
                }
                else
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }
            catch
            {
                throw new Exception("Logout is unavailable, please try again later");
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult Update()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Update(UserUpdateDTO user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userId == null)
                    {
                        ModelState.AddModelError("", "Your user id could not be retrieved");
                        return View();
                    }

                    User updatedUser = new()
                    {
                        Id = userId,
                        UserName = user.UserName ?? null,
                        Email = user.Email ?? null,
                        PhoneNumber = user.Phone ?? null,
                        FullName = user.FullName ?? null,
                        CEP = user.CEP ?? 0,
                        CartId = 0
                    };

                    await _userDbService.UpdateUser(updatedUser, user.Password);
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(user);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    ViewData["Error"] = "Your user id could not be retrieved";
                    return View();
                }

                await _userDbService.DeleteUser(userId);
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                throw new Exception("Delete User is unavailable, please try again later");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Purchases()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                ViewData["Error"] = "Your user id could not be retrieved";
                return View();
            }

            var purchases = await _salesTransactionDbService.GetTransactionFromUser(userId);
            return View(purchases);
        }
    }
}
