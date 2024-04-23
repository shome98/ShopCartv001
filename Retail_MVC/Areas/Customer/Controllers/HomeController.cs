using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models;
using Retail_MVC.Utility;
using System.Diagnostics;
using System.Security.Claims;

namespace Retail_MVC.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Product> productList = await _unitOfWork.Product.GetAllAsync(includeProperties: "Category");
            return View(productList);
        }

		public async Task<IActionResult> Details(int productId)
		{
            ShoppingCart cart = new()
            {
                Product = await _unitOfWork.Product.GetAsync(u => u.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId
            };
			return View(cart);
		}
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
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
                (await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == userId)).Count());
            }


            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Search(string searchTerm)
        {
            IEnumerable<Product> filtered = await _unitOfWork.Product.GetAllAsync();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var result = filtered.Where(u => string.Equals(u.Name, searchTerm, StringComparison.CurrentCultureIgnoreCase) || string.Equals(u.Description, searchTerm, StringComparison.CurrentCultureIgnoreCase)).ToList();
                return View("Index", result);
            }
            return View("Index", filtered);

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
