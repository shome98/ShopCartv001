using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Retail_MVC.Models;
using Retail_MVC.Models.ViewModels;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Retail_MVC.Utility;
using Retail_MVC.Services;

namespace Retail_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Vendor)]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService,ICategoryService categoryService)
        {
            _productService=productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var prodobj = await _productService.GetAllAsync("Category");
            return View(prodobj);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = (await _categoryService.GetAllAsync()).Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() }),
                Product = new Product()
            };
            if(id==null || id==0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = await _productService.GetAsync(id);
                return View(productVM);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(ProductVM productVM, IFormFile? file)
        { 
            if(ModelState.IsValid)
            {
                var prod = _productService.ImageHandle(productVM, file);
                if (prod.Product.Id == 0)
                {
                    await _productService.AddAsync(prod.Product);
                }
                else
                {
                    await _productService.UpdateAsync(prod.Product);
                }
                return RedirectToAction("Index", "Product");
            }
            else
            {
                productVM.CategoryList = (await _categoryService.GetAllAsync()).Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product prodfromdb = await _productService.GetAsync(id);
            if (prodfromdb == null)
            {
                return NotFound();
            }
            return View(prodfromdb);
        }

        [HttpPost,ActionName("Delete")]
        public async Task<IActionResult> DelectPost(int id)
        {
            Product prodfromdb = await _productService.GetAsync(id);
            if (prodfromdb == null)
            {
                return NotFound();
            }
            await _productService.RemoveAsync(prodfromdb);
            return RedirectToAction("Index", "Product");
        }
    }
}