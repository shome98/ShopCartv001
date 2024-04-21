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
using System.Runtime.CompilerServices;

namespace Retail_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class VendorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public VendorController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
           
        }


        public async Task<IActionResult> Index()
        {
            //List<Vendor> vendobj= await _unitOfWork.Vendor.GetAllAsync();
            var vendobj = await _unitOfWork.Vendor.GetAllAsync();
            return View(vendobj);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            
            if(id==null || id==0)
            {
                return View(new Vendor());
            }

            else
            {
                Vendor vendobj = await _unitOfWork.Vendor.GetAsync(u => u.Id == id);
                return View(vendobj);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(Vendor vendobj)
        {
            //VendorVM.Vendor.Id = 0;
            if(ModelState.IsValid)
            {
                if(vendobj.Id == 0)
                {
                    await _unitOfWork.Vendor.AddAsync(vendobj);
                }
                else
                {
                    await _unitOfWork.Vendor.UpdateAsync(vendobj);
                }
                
                await _unitOfWork.SaveAsync();
                return RedirectToAction("Index", "Vendor");
            }
            else
            {
                
                return View(vendobj);
            }
           
        }

       

        public async Task<IActionResult> Delete(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Vendor prodfromdb = await _unitOfWork.Vendor.GetAsync(u=>u.Id==id);
            if (prodfromdb == null)
            {
                return NotFound();
            }
            return View(prodfromdb);
        }

        [HttpPost,ActionName("Delete")]
        public async Task<IActionResult> DelectPost(int id)
        {
            Vendor prodCategory = await _unitOfWork.Vendor.GetAsync(u => u.Id == id);
            if (prodCategory == null)
            {
                return NotFound();
            }
            await _unitOfWork.Vendor.RemoveAsync(prodCategory);
            await _unitOfWork.SaveAsync();
            return RedirectToAction("Index", "Vendor");
        }


        


    }
}
