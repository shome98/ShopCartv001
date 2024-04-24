using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models;
using Retail_MVC.Models.ViewModels;
using Retail_MVC.Utility;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Retail_MVC.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                ShoppingCartVM = new()
                {
                    ShoppingCartList = await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                    OrderHeader = new()
                };

                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    cart.Price = GetPrice(cart);
                    ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }

                return View(ShoppingCartVM);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<IActionResult> Summary()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                ShoppingCartVM = new()
                {
                    ShoppingCartList = await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                    OrderHeader = new()
                };

                ShoppingCartVM.OrderHeader.ApplicationUser = await _unitOfWork.ApplicationUser.GetAsync(u => u.Id == userId);

                ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
                ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
                ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
                ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
                ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
                ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    cart.Price = GetPrice(cart);
                    ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }

                return View(ShoppingCartVM);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPOST(ShoppingCartVM ShoppingCartVM)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                ShoppingCartVM = new()
                {
                    ShoppingCartList = await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                    OrderHeader = new()
                };

                ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
                ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

                ApplicationUser applicationUser = await _unitOfWork.ApplicationUser.GetAsync(u => u.Id == userId);

                ShoppingCartVM.OrderHeader.Name = applicationUser.Name;
                ShoppingCartVM.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
                ShoppingCartVM.OrderHeader.StreetAddress = applicationUser.StreetAddress;
                ShoppingCartVM.OrderHeader.City = applicationUser.City;
                ShoppingCartVM.OrderHeader.State = applicationUser.State;
                ShoppingCartVM.OrderHeader.PostalCode = applicationUser.PostalCode;

                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    cart.Price = GetPrice(cart);
                    ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }

                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;

                await _unitOfWork.OrderHeader.AddAsync(ShoppingCartVM.OrderHeader);
                await _unitOfWork.SaveAsync();

                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    OrderDetail orderDetail = new()
                    {
                        ProductId = cart.ProductId,
                        OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                        Price = cart.Price,
                        Count = cart.Count
                    };
                    await _unitOfWork.OrderDetail.AddAsync(orderDetail);
                    await _unitOfWork.SaveAsync();
                }

                var domain = "https://localhost:7163/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment"
                };

                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "inr",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name,
                                Description = item.Product.Description
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);

                await _unitOfWork.OrderHeader.UpdateStripePaymentIdAsync(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                await _unitOfWork.SaveAsync();

                Response.Headers.Add("Location", session.Url);
                HttpContext.Session.SetInt32(SD.SessionCart, 0);

                return new StatusCodeResult(303);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            try
            {
                OrderHeader orderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == id, includeProperties: "ApplicationUser");

                if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
                {
                    var service = new SessionService();
                    Session session = service.Get(orderHeader.SessionId);

                    if (session.PaymentStatus.ToLower() == "paid")
                    {
                        await _unitOfWork.OrderHeader.UpdateStripePaymentIdAsync(id, session.Id, session.PaymentIntentId);
                        await _unitOfWork.OrderHeader.UpdateStatusAsync(id, SD.StatusApproved, SD.PaymentStatusApproved);
                        await _unitOfWork.SaveAsync();
                    }
                }

                var shoppingCarts = await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == orderHeader.ApplicationUserId);
                await _unitOfWork.ShoppingCart.RemoveRangeAsync(shoppingCarts);
                await _unitOfWork.SaveAsync();

                return View(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<IActionResult> plus(int cartId)
        {
            try
            {
                var cartFromDb = await _unitOfWork.ShoppingCart.GetAsync(u => u.Id == cartId);
                cartFromDb.Count += 1;
                await _unitOfWork.ShoppingCart.UpdateAsync(cartFromDb);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<IActionResult> minus(int cartId)
        {
            try
            {
                var cartFromDb = await _unitOfWork.ShoppingCart.GetAsync(u => u.Id == cartId);

                if (cartFromDb.Count <= 1)
                {
                    await _unitOfWork.ShoppingCart.RemoveAsync(cartFromDb);
                    HttpContext.Session.SetInt32(SD.SessionCart, (await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == cartFromDb.ApplicationUserId)).Count());
                }
                else
                {
                    cartFromDb.Count -= 1;
                    await _unitOfWork.ShoppingCart.UpdateAsync(cartFromDb);
                }

                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<IActionResult> remove(int cartId)
        {
            try
            {
                var cartFromDb = await _unitOfWork.ShoppingCart.GetAsync(u => u.Id == cartId);
                await _unitOfWork.ShoppingCart.RemoveAsync(cartFromDb);
                HttpContext.Session.SetInt32(SD.SessionCart, (await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == cartFromDb.ApplicationUserId)).Count());
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private double GetPrice(ShoppingCart shoppingCart)
        {
            return shoppingCart.Product.Price;
        }
    }
}

