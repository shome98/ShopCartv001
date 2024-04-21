using Retail_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retail_MVC.DataAccess.Repository.IRepository
{
    public interface ICourierRepository : IRepository<Courier>
    {

        Task UpdateAsync(Courier obj);
    }
}
