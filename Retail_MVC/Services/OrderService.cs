// Service interface for order operations
using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models.ViewModels;
using Retail_MVC.Models;
using Retail_MVC.Utility;
using System.Security.Claims;
using Stripe.Checkout;

public interface IOrderService
{
    Task<IEnumerable<OrderHeader>> GetAllOrdersAsync(ClaimsPrincipal user);
    Task<OrderVM> GetOrderDetailsAsync(int orderId);
    Task UpdateOrderDetailAsync(OrderVM orderVM);
    Task StartProcessingAsync(int orderId);
    Task ShipOrderAsync(OrderVM orderVM);
    Task CancelOrderAsync(OrderVM orderVM);
    Task<string> CreatePaymentSessionAsync(int orderId, HttpRequest Request);
    Task ProcessPaymentConfirmationAsync(int orderHeaderId);
}

// Implementation of the order service
public class OrderService

{   private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<OrderHeader>> GetAllOrdersAsync(ClaimsPrincipal user)
    {
        try
        {
            string userId = user.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (user.IsInRole(SD.Role_Admin) || user.IsInRole(SD.Role_Courier))
            {
                return await _unitOfWork.OrderHeader.GetAllAsync(includeProperties: "ApplicationUser");
            }
            else
            {
                return await _unitOfWork.OrderHeader.GetAllAsync(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            throw new Exception("Failed to retrieve orders.", ex);
        }
    }

    public async Task<OrderVM> GetOrderDetailsAsync(int orderId)
    {
        try
        {
            OrderHeader orderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == orderId, includeProperties: "ApplicationUser");
            IEnumerable<OrderDetail> orderDetails = await _unitOfWork.OrderDetail.GetAllAsync(u => u.OrderHeaderId == orderId, includeProperties: "Product");

            return new OrderVM
            {
                OrderHeader = orderHeader,
                OrderDetail = orderDetails
            };
        }
        catch (Exception ex)
        {
            // Log the exception
            throw new Exception("Failed to retrieve order details.", ex);
        }
    }

    public async Task UpdateOrderDetailAsync(OrderVM orderVM)
    {
        try
        {
            OrderHeader orderHeaderFromDb = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == orderVM.OrderHeader.Id);
            orderHeaderFromDb.Name = orderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = orderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = orderVM.OrderHeader.City;
            orderHeaderFromDb.State = orderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = orderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            }
            await _unitOfWork.OrderHeader.UpdateAsync(orderHeaderFromDb);
            await _unitOfWork.SaveAsync();
        }
        catch (Exception ex)
        {
            // Log the exception
            throw new Exception("Failed to update order details.", ex);
        }
    }

    public async Task StartProcessingAsync(int orderId)
    {
        try
        {
            await _unitOfWork.OrderHeader.UpdateStatusAsync(orderId, SD.StatusInProcess);
            await _unitOfWork.SaveAsync();
        }
        catch (Exception ex)
        {
            // Log the exception
            throw new Exception("Failed to start processing the order.", ex);
        }
    }

    public async Task ShipOrderAsync(OrderVM orderVM)
    {
        try
        {
            OrderHeader orderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == orderVM.OrderHeader.Id);
            if (orderHeader != null)
            {
                orderHeader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
                orderHeader.Carrier = orderVM.OrderHeader.Carrier;
                orderHeader.OrderStatus = SD.StatusShipped;
                orderHeader.ShippingDate = DateTime.Now;
                await _unitOfWork.OrderHeader.UpdateAsync(orderHeader);
                await _unitOfWork.SaveAsync();
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            throw new Exception("Failed to ship the order.", ex);
        }
    }

    public async Task CancelOrderAsync(OrderVM orderVM)
    {
        try
        {
            OrderHeader orderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == orderVM.OrderHeader.Id);
            if (orderHeader != null)
            {
                await _unitOfWork.OrderHeader.UpdateStatusAsync(orderHeader.Id, SD.StatusCancelled,
                    orderHeader.PaymentStatus == SD.PaymentStatusApproved ? SD.StatusRefunded : SD.StatusCancelled);
                await _unitOfWork.SaveAsync();
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            throw new Exception("Failed to cancel the order.", ex);
        }
    }

    /*
    public async Task<string> CreatePaymentSessionAsync(int orderId, HttpRequest Request)
    {
        try
        {
            OrderHeader orderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == orderId);
            if (orderHeader != null)
            {
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderHeader.Id}",
                    CancelUrl = domain + $"admin/order/details?orderHeaderId={orderHeader.Id}",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment"
                };
                foreach (var item in orderHeader.OrderDetail)
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
                await _unitOfWork.OrderHeader.UpdateStripePaymentIdAsync(orderHeader.Id, session.Id, session.PaymentIntentId);
                await _unitOfWork.SaveAsync();
                return session.Id;
            }
            else
            {
                throw new Exception($"Order with ID {orderId} not found.");
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            throw new Exception("Failed to create payment session.", ex);
        }
    }

    public async Task ProcessPaymentConfirmationAsync(int orderHeaderId)
    {
        try
        {
            OrderHeader orderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == orderHeaderId);
            if (orderHeader != null)
            {
                if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
                {
                    var service = new SessionService();
                    Session session = service.Get(orderHeader.StripePaymentId);

                    if (session.PaymentStatus.ToLower() == "paid")
                    {
                        await _unitOfWork.OrderHeader.UpdateStripePaymentIdAsync(orderHeaderId, session.Id, session.PaymentIntentId);
                        await _unitOfWork.OrderHeader.UpdateStatusAsync(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                        await _unitOfWork.SaveAsync();
                    }
                }
                else
                {
                    throw new Exception($"Payment for order with ID {orderHeaderId} is not delayed.");
                }
            }
            else
            {
                throw new Exception($"Order with ID {orderHeaderId} not found.");
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            throw new Exception("Failed to process payment confirmation.", ex);
        }
    }*/

}
