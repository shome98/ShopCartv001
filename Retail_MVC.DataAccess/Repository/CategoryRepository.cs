using Retail_MVC.DataAccess.Data;
using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models;

namespace Retail_MVC.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public async Task UpdateAsync(Category obj)
        {
            _db.Categories.Update(obj);
            await _db.SaveChangesAsync();
        }
    }
}
