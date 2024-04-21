using Retail_MVC.DataAccess.Data;
using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models;

namespace Retail_MVC.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _db;

        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public async Task UpdateAsync(ShoppingCart obj)
        {
            _db.ShoppingCarts.Update(obj);
            await _db.SaveChangesAsync();
        }
    }
}
