using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

/*namespace Retail_MVC.DataAccess.Repository.IRepository
{
	public interface IRepository<T> where T:class
	{
		//T=Category
		IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null, string? includeProperties = null);
		T Get(Expression<Func<T,bool>> filter, string? includeProperties = null,bool tracked=false);
		void Add(T entity);
		void Remove(T entity);

		void RemoveRange(IEnumerable<T> entity);

	}
}*/

namespace Retail_MVC.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null, string includeProperties = null);
        Task<T> GetAsync(Expression<Func<T, bool>> filter, string includeProperties = null, bool tracked = false);
        Task AddAsync(T entity);
        Task RemoveAsync(T entity);
        Task RemoveRangeAsync(IEnumerable<T> entity);
    }
}
