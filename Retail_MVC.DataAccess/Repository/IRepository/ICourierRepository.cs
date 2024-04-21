using Retail_MVC.Models;


namespace Retail_MVC.DataAccess.Repository.IRepository
{
    public interface ICourierRepository : IRepository<Courier>
    {

        Task UpdateAsync(Courier obj);
    }
}
