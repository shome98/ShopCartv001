using Microsoft.AspNetCore.Mvc;
using Retail_MVC.Models;
using Retail_MVC.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Retail_MVC.Utility;
using Retail_MVC.Services;
using Retail_MVC.DataAccess.Repository;

namespace Retail_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin+","+SD.Role_Vendor)]
    
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService=categoryService;
        }
        public async Task<IActionResult> Index()
        {
            var objCategoryModel = await _categoryService.GetAllAsync();
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
                await _categoryService.AddAsync(obj);
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
            Category obj=await _categoryService.GetAsync(id);
            if(obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Category obj)
        {
            if(ModelState.IsValid)
            {
                await _categoryService.UpdateAsync(obj);
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
			Category objectCategory = await _categoryService.GetAsync(id);
			if (objectCategory == null)
			{
				return NotFound();
			}
			return View(objectCategory);
		}
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int id)
        {
			Category objectCategory = await _categoryService.GetAsync(id);
            if(objectCategory == null)
            {
                return NotFound();
            }
            await _categoryService.RemoveAsync(objectCategory);
            return RedirectToAction("Index", "Category");
		}
    }
}
