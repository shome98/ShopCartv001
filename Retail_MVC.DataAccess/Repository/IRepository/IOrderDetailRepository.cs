using Retail_MVC.Models;

namespace Retail_MVC.DataAccess.Repository.IRepository
{
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {
        Task UpdateAsync(OrderDetail obj);
    }
}
