using Microsoft.AspNetCore.Mvc;
using Retail_MVC.Models;
using Retail_MVC.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Retail_MVC.Utility;

namespace Retail_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin+","+SD.Role_Vendor)]
    
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var objCategoryModel = await _unitOfWork.Category.GetAllAsync();
                return View(objCategoryModel);
            }
            catch (Exception ex)
            {
                throw new Exception("An occured while getting the catagories ", ex);
            }
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _unitOfWork.Category.AddAsync(obj);
                    await _unitOfWork.SaveAsync();
                    return RedirectToAction("Index", "Category");
                }
                catch(Exception ex)
                {
                    throw new Exception("An error occurred while adding a new catagory!!!",ex);
                }
            }

            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            try { 
            Category objectCategory = await _unitOfWork.Category.GetAsync(u => u.Id == id);
                if (objectCategory == null)
                {
                    return NotFound();
                }
                return View(objectCategory);
            }
            catch(Exception ex)
            {
                throw new Exception($"An error occurred while getting the particyular category with id {id}!!!", ex);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _unitOfWork.Category.UpdateAsync(obj);
                    await _unitOfWork.SaveAsync();
                    return RedirectToAction("Index", "Category");
                }
                catch(Exception ex)
                {
                    throw new Exception ($"An error occurred while updating the category with id ${obj.Id}!!!",ex);
                }
            }

            return View();
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            try
            {
                Category objectCategory = await _unitOfWork.Category.GetAsync(u => u.Id == id);
                if (objectCategory == null)
                {
                    return NotFound();
                }
                return View(objectCategory);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while getting the particyular category with id {id}!!!", ex);
            }
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int id)
        {
            try
            {
                Category objectCategory = await _unitOfWork.Category.GetAsync(u => u.Id == id);
                if (objectCategory == null)
                {
                    return NotFound();
                }
                await _unitOfWork.Category.RemoveAsync(objectCategory);
                await _unitOfWork.SaveAsync();
                return RedirectToAction("Index", "Category");
            }
            catch(Exception ex) 
            {
                throw new Exception($"An error occurred while deleting the category with id {id}!!!", ex);
            }
        }
    }
}
