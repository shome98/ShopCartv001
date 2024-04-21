using Retail_MVC.Models;

namespace Retail_MVC.DataAccess.Repository.IRepository
{
    public interface IVendorRepository : IRepository<Vendor>
    {
        Task UpdateAsync(Vendor obj);
    }
}
