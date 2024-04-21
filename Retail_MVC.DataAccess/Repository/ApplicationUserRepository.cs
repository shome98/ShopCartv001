using Retail_MVC.DataAccess.Data;
using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models;

namespace Retail_MVC.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext _db;

        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
