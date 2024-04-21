using Retail_MVC.DataAccess.Data;
using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models;

namespace Retail_MVC.DataAccess.Repository
{
    public class CourierRepository : Repository<Courier>, ICourierRepository
    {
        private readonly ApplicationDbContext _db;
        public CourierRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public async Task UpdateAsync(Courier obj)
        {
             _db.Couriers.Update(obj);
            await _db.SaveChangesAsync();
        }
    }
}
