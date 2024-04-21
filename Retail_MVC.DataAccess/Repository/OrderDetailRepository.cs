using Retail_MVC.DataAccess.Data;
using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models;

namespace Retail_MVC.DataAccess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderDetailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public async Task UpdateAsync(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
            await _db.SaveChangesAsync();
        }
    }
}
