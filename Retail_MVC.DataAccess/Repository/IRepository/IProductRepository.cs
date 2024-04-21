using Retail_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*namespace Retail_MVC.DataAccess.Repository.IRepository
{
	public interface IProductRepository:IRepository<Product>
	{
		
		void Update(Product obj);
	}
}
*/
namespace Retail_MVC.DataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        Task UpdateAsync(Product obj);
    }
}
