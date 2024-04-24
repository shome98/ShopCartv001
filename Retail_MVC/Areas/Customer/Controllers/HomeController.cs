using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models;
using Retail_MVC.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Retail_MVC.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                IEnumerable<Product> productList = await _unitOfWork.Product.GetAllAsync(includeProperties: "Category");
                return View(productList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving products.");
                return View("Error");
            }
        }

        public async Task<IActionResult> Details(int productId)
        {
            try
            {
                ShoppingCart cart = new()
                {
                    Product = await _unitOfWork.Product.GetAsync(u => u.Id == productId, includeProperties: "Category"),
                    Count = 1,
                    ProductId = productId
                };
                return View(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving product details.");
                return View("Error");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                shoppingCart.ApplicationUserId = userId;

                ShoppingCart cartFromDb = await _unitOfWork.ShoppingCart.GetAsync(u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);
                if (cartFromDb != null)
                {
                    cartFromDb.Count += shoppingCart.Count;
                    await _unitOfWork.ShoppingCart.UpdateAsync(cartFromDb);
                }
                else
                {
                    await _unitOfWork.ShoppingCart.AddAsync(shoppingCart);
                    HttpContext.Session.SetInt32(SD.SessionCart,
                    (await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == userId)).Count() + 1);
                }

                await _unitOfWork.SaveAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding product to cart.");
                return View("Error");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Filter(string searchString)
        {
            try
            {
                var allProduct = await _unitOfWork.Product.GetAllAsync();

                if (!string.IsNullOrEmpty(searchString))
                {
                    var filteredResultNew = allProduct.Where(n => string.Equals(n.Name, searchString, StringComparison.CurrentCultureIgnoreCase)).ToList();
                    return View("Index", filteredResultNew);
                }

                return View("Index", allProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while filtering products.");
                return View("Error");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
