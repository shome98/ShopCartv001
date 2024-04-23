using Retail_MVC.DataAccess.Repository;
using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models;

namespace Retail_MVC.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task AddAsync(Category obj)
        {
            try
            {
                await _unitOfWork.Category.AddAsync(obj);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding a new catagory!!!", ex);
            }

        }
        public async Task<Category> GetAsync(int id)
        {
            try
            {
                return await _unitOfWork.Category.GetAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while getting the particyular category with id {id}!!!", ex);
            }
        }
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            try
            {
                var objCategoryModel = await _unitOfWork.Category.GetAllAsync();
                return objCategoryModel;
            }
            catch (Exception ex)
            {
                throw new Exception("An occured while getting the catagories ", ex);
            }
        }
        public async Task RemoveAsync(Category obj)
        {
            try
            {
                await _unitOfWork.Category.RemoveAsync(obj);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while deleting the category with id {obj.Id}!!!", ex);
            }
        }
        public async Task UpdateAsync(Category obj)
        {
            try
            {
                await _unitOfWork.Category.UpdateAsync(obj);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the category with id ${obj.Id}!!!", ex);
            }
        }
    }
}
