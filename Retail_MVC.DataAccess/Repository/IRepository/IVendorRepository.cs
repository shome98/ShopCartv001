using Retail_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*namespace Retail_MVC.DataAccess.Repository.IRepository
{
	public interface IVendorRepository:IRepository<Vendor>
	{
		
		void Update(Vendor obj);
	}
}*/
namespace Retail_MVC.DataAccess.Repository.IRepository
{
    public interface IVendorRepository : IRepository<Vendor>
    {
        Task UpdateAsync(Vendor obj);
    }
}
