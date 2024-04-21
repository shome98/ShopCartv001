using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models;
using Retail_MVC.Models.ViewModels;
using System.Collections.Generic;
using System.Data;
using Retail_MVC.DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Retail_MVC.Utility;

namespace Retail_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Vendor)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }


        public async Task<IActionResult> Index()
        {
            //List<Product> prodobj= await _unitOfWork.Product.GetAllAsync(includeProperties:"Category");
            var prodobj = await _unitOfWork.Product.GetAllAsync(includeProperties: "Category");
            return View(prodobj);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = (await _unitOfWork.Category.GetAllAsync()).Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() }),
                Product = new Product()
            };
            if(id==null || id==0)
            {
                return View(productVM);
            }

            else
            {
                productVM.Product = await _unitOfWork.Product.GetAsync(u => u.Id == id);
                return View(productVM);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(ProductVM productVM, IFormFile? file)
        {
            productVM.Product.Id = 0;
            if(ModelState.IsValid)
            {
                string wwwRootPath=_webHostEnvironment.WebRootPath;
                if(file != null)
                {
                    string fileName=Guid.NewGuid().ToString()+ Path.GetExtension(file.FileName);
                    string productPath=Path.Combine(wwwRootPath, @"images\product\");

                    if(!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
						var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream=new FileStream(Path.Combine(productPath,fileName),FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }
                if(productVM.Product.Id == 0)
                {
                    await _unitOfWork.Product.AddAsync(productVM.Product);
                }
                else
                {
                    await _unitOfWork.Product.UpdateAsync(productVM.Product);
                }
                
                await _unitOfWork.SaveAsync();
                return RedirectToAction("Index", "Product");
            }
            else
            {
                productVM.CategoryList = (await _unitOfWork.Category.GetAllAsync()).Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }
           
        }

       

        public async Task<IActionResult> Delete(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product prodfromdb = await _unitOfWork.Product.GetAsync(u=>u.Id==id);
            if (prodfromdb == null)
            {
                return NotFound();
            }
            return View(prodfromdb);
        }

        [HttpPost,ActionName("Delete")]
        public async Task<IActionResult> DelectPost(int id)
        {
            Product prodCategory = await _unitOfWork.Product.GetAsync(u => u.Id == id);
            if (prodCategory == null)
            {
                return NotFound();
            }
            await _unitOfWork.Product.RemoveAsync(prodCategory);
            await _unitOfWork.SaveAsync();
            return RedirectToAction("Index", "Product");
        }


        


    }
}
