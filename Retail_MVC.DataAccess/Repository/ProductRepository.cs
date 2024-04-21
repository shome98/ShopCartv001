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
	public class ProductRepository : Repository<Product>, IProductRepository
	{
		private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db):base(db) 
        {
            _db=db;
        }
       

		public void Update(Product obj)
		{
			var objFromDb=_db.products.FirstOrDefault(u=>u.Id==obj.Id);
			if(objFromDb!=null)
			{
				objFromDb.Name=obj.Name;
				objFromDb.Description=obj.Description;
				objFromDb.Price=obj.Price;
				if(obj.ImageUrl!=null)
				{
					objFromDb.ImageUrl=obj.ImageUrl;
				}
			}
		}

        
    }
}*/

using Microsoft.EntityFrameworkCore;
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
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public async Task UpdateAsync(Product obj)
        {
            var objFromDb = await _db.products.FirstOrDefaultAsync(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = obj.Name;
                objFromDb.Description = obj.Description;
                objFromDb.Price = obj.Price;
                if (obj.ImageUrl != null)
                {
                    objFromDb.ImageUrl = obj.ImageUrl;
                }
            }
            await _db.SaveChangesAsync();
        }
    }
}

