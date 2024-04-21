using Retail_MVC.Models;

namespace Retail_MVC.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        Task UpdateAsync(ShoppingCart obj);
    }
}
