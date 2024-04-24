using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models;

namespace Retail_MVC.Services
{
    public class OrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<OrderHeader>> GetAllAsync()
        {
            try
            {
                return await _unitOfWork.OrderHeader.GetAllAsync(includeProperties: "ApplicationUser");
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while getting the order header!!!", ex);
            }
        }

        public async Task<OrderHeader> GetAsync(int id)
        {
            try
            {
                return await _unitOfWork.OrderHeader.GetAsync(o => o.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while getting the orderheader with id {id}", ex);
            }
        }

        public async Task<IEnumerable<OrderDetail>> GetOrderDetailsAsync(int id)
        {
            try
            {
                return await _unitOfWork.OrderDetail.GetAllAsync(o => o.OrderHeaderId == id, includeProperties: "Product");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while getting the orderdetails with orderheaderId {id}", ex);
            }
        }
    }
}
