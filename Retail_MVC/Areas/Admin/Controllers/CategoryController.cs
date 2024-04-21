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
            var objCategoryModel = await _unitOfWork.Category.GetAllAsync();
            return View(objCategoryModel);
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
                await _unitOfWork.Category.AddAsync(obj);
                await _unitOfWork.SaveAsync();
                return RedirectToAction("Index", "Category");
            }

            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category objectCategory = await _unitOfWork.Category.GetAsync(u => u.Id == id);
            if (objectCategory == null)
            {
                return NotFound();
            }
            return View(objectCategory);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.Category.UpdateAsync(obj);
                await _unitOfWork.SaveAsync();
                return RedirectToAction("Index", "Category");
            }

            return View();
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category objectCategory = await _unitOfWork.Category.GetAsync(u => u.Id == id);
            if (objectCategory == null)
            {
                return NotFound();
            }
            return View(objectCategory);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int id)
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
    }
}
