using Retail_MVC.Models;

namespace Retail_MVC.DataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        Task UpdateAsync(Product obj);
    }
}
