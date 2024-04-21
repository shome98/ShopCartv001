using Retail_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*namespace Retail_MVC.DataAccess.Repository.IRepository
{
	public interface IOrderHeaderRepository:IRepository<OrderHeader>
	{
		
		void Update(OrderHeader obj);
		void UpdateStatus(int id, string orderStatus,string? paymentStatus=null);
		void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId);
	}
}
*/

namespace Retail_MVC.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        Task UpdateAsync(OrderHeader obj);
        Task UpdateStatusAsync(int id, string orderStatus, string? paymentStatus=null);
        Task UpdateStripePaymentIdAsync(int id, string sessionId, string paymentIntentId);
    }
}
