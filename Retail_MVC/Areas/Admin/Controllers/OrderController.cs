using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models;
using Retail_MVC.Models.ViewModels;
using Retail_MVC.Utility;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;


namespace Retail_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
   
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                IEnumerable<OrderHeader> prodobj;
                if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Courier))
                {
                    prodobj = await _unitOfWork.OrderHeader.GetAllAsync(includeProperties: "ApplicationUser");
                }
                else
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                    prodobj = await _unitOfWork.OrderHeader.GetAllAsync(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
                }
                return View(prodobj);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [Authorize]
		public async Task<IActionResult> Details(int orderId)
        {
            try
            {
                OrderVM = new()
                {
                    OrderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                    OrderDetail = await _unitOfWork.OrderDetail.GetAllAsync(u => u.OrderHeaderId == orderId, includeProperties: "Product")
                };
                return View(OrderVM);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpPost]
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Courier)]
        public async Task<IActionResult> UpdateOrderDetail(int orderId)
        {
            try
            {
                var orderHeaderFromDb = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == OrderVM.OrderHeader.Id);
                orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
                orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
                orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
                orderHeaderFromDb.City = OrderVM.OrderHeader.City;
                orderHeaderFromDb.State = OrderVM.OrderHeader.State;
                orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
                if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
                {
                    orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
                }
                if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
                {
                    orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
                }
                await _unitOfWork.OrderHeader.UpdateAsync(orderHeaderFromDb);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Order Details Updated Successfully.";


                return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
            }
        }

        [HttpPost]
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Courier)]
        public async Task<IActionResult> StartProcessing()
        {
            try
            {
                await _unitOfWork.OrderHeader.UpdateStatusAsync(OrderVM.OrderHeader.Id, SD.StatusInProcess);
                await _unitOfWork.SaveAsync();
                TempData["Success"] = "Order Details Updated Successfully";
                return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
            }
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Courier)]
        public async Task<IActionResult> ShipOrder()
        {
            try
            {
                var orderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == OrderVM.OrderHeader.Id);
                if (orderHeader != null)
                {
                    if (orderHeader.TrackingNumber != null && orderHeader.Carrier != null)
                    {
                        orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
                        orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
                        orderHeader.OrderStatus = SD.StatusShipped;
                        orderHeader.ShippingDate = DateTime.Now;
                        if (orderHeader.PaymentStatus == SD.PaymentStatusPending)
                        {
                            orderHeader.PaymentDueDate = DateOnly.Parse(DateTime.Now.AddDays(3).ToString());

                        }
                        await _unitOfWork.OrderHeader.UpdateAsync(orderHeader);
                        await _unitOfWork.SaveAsync();
                        TempData["Success"] = "Order Shipped Successfully";
                        return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
                    }
                    else
                    {
                        TempData["Please"] = "Please enter the carrier and tarcking id";
                        return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
            }
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Courier)]
        public async Task<IActionResult> CancelOrder()
        {
            try
            {
                var orderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == OrderVM.OrderHeader.Id);
                if (orderHeader != null)
                {
                    if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
                    {
                        var options = new RefundCreateOptions
                        {
                            Reason = RefundReasons.RequestedByCustomer,
                            PaymentIntent = orderHeader.PaymentIntentId,
                        };
                        var service = new RefundService();
                        Refund refund = await service.CreateAsync(options);
                        await _unitOfWork.OrderHeader.UpdateStatusAsync(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
                    }
                    else
                    {
                        await _unitOfWork.OrderHeader.UpdateStatusAsync(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
                    }
                    await _unitOfWork.SaveAsync();
                    TempData["Success"] = "Order Cancelled Successfully";
                    return RedirectToAction(nameof(Details), new { orderId = orderHeader.Id });
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
            }
        }

        [ActionName("Details")]
        [HttpPost]
        public async Task<IActionResult> Details_PAY_NOW()
        {
            try
            {
                OrderVM.OrderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
                OrderVM.OrderDetail = await _unitOfWork.OrderDetail.GetAllAsync(u => u.OrderHeaderId == OrderVM.OrderHeader.Id, includeProperties: "Product");

                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
                    CancelUrl = domain + $"admin/order/details?orderHeaderId={OrderVM.OrderHeader.Id}",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment"
                };
                foreach (var item in OrderVM.OrderDetail)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name,
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new SessionService();
                Session session = service.Create(options);
                await _unitOfWork.OrderHeader.UpdateStripePaymentIdAsync(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                await _unitOfWork.SaveAsync();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
            }
        }

        public async Task<IActionResult> PaymentConfirmation(int orderHeaderId)
        {
            try
            {
                OrderHeader orderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == orderHeaderId);
                if (orderHeader.PaymentStatus == SD.PaymentStatusPending)
                {
                    //delayed payment
                    var service = new SessionService();
                    Session session = service.Get(orderHeader.SessionId);

                    if (session.PaymentStatus.ToLower() == "paid")
                    {
                        await _unitOfWork.OrderHeader.UpdateStripePaymentIdAsync(orderHeaderId, session.Id, session.PaymentIntentId);
                        await _unitOfWork.OrderHeader.UpdateStatusAsync(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                        await _unitOfWork.SaveAsync();
                    }
                }
                return View(orderHeaderId);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Details), new { orderId = orderHeaderId });
            }
        }
    }
}
