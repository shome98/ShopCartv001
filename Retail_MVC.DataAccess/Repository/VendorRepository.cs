/*using Retail_MVC.DataAccess.Data;
using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retail_MVC.DataAccess.Repository
{
	public class VendorRepository : Repository<Vendor>, IVendorRepository
    {
		private readonly ApplicationDbContext _db;
        public VendorRepository(ApplicationDbContext db):base(db) 
        {
            _db=db;
        }
       

		public void Update(Vendor obj)
		{
			_db.vendors.Update(obj);
		}
	}
}*/

using Retail_MVC.DataAccess.Data;
using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retail_MVC.DataAccess.Repository
{
    public class VendorRepository : Repository<Vendor>, IVendorRepository
    {
        private readonly ApplicationDbContext _db;

        public VendorRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public async Task UpdateAsync(Vendor obj)
        {
            _db.vendors.Update(obj);
            await _db.SaveChangesAsync();
        }
    }
}
