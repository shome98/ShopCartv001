using Retail_MVC.Models;
using Retail_MVC.Models.ViewModels;

namespace Retail_MVC.Services
{
    public interface IProductService
    {
        Task AddAsync(Product obj);
        Task<IEnumerable<Product>> GetAllAsync(string include);
        Task<Product> GetAsync(int? id);
        ProductVM ImageHandle(ProductVM productVM, IFormFile? file);
        Task RemoveAsync(Product obj);
        Task UpdateAsync(Product obj);
    }
}