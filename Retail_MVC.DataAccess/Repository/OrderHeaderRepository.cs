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
	public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
	{
		private readonly ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db):base(db) 
        {
            _db=db;
        }
       

		public void Update(OrderHeader obj)
		{
			_db.OrderHeaders.Update(obj);
		}

		

		void IOrderHeaderRepository.UpdateStatus(int id, string orderStatus, string? paymentStatus)
		{
			var orderFromDb=_db.OrderHeaders.FirstOrDefault(u=>u.Id==id);
			if (orderFromDb != null)
			{
				orderFromDb.OrderStatus=orderStatus;
				if(!string.IsNullOrEmpty(paymentStatus))
				{
					orderFromDb.PaymentStatus=paymentStatus;
				}
			}
		}

		void IOrderHeaderRepository.UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
		{
			var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
			if (!string.IsNullOrEmpty(sessionId))
			{
				orderFromDb.SessionId = sessionId;
			}
			if (!string.IsNullOrEmpty(paymentIntentId))
			{
				orderFromDb.PaymentIntentId=paymentIntentId;
				orderFromDb.PaymentDate = DateTime.Now;
			}
		}
	}
}
*/

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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public async Task UpdateAsync(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int id, string orderStatus, string paymentStatus)
        {
            var orderFromDb = await _db.OrderHeaders.FirstOrDefaultAsync(u => u.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
            await _db.SaveChangesAsync();
        }

        public async Task UpdateStripePaymentIdAsync(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = await _db.OrderHeaders.FirstOrDefaultAsync(u => u.Id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                orderFromDb.PaymentIntentId = paymentIntentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
            await _db.SaveChangesAsync();
        }
    }
}
