using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models;
using Retail_MVC.Utility;
using System.Runtime.CompilerServices;

namespace Retail_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CourierController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CourierController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }


        public async Task<IActionResult> Index()
        {
            var vendobj = await _unitOfWork.Courier.GetAllAsync();
            return View(vendobj);
        }

        public async Task<IActionResult> Upsert(int? id)
        {

            if (id == null || id == 0)
            {
                return View(new Courier());
            }

            else
            {
                Courier vendobj = await _unitOfWork.Courier.GetAsync(u => u.Id == id);
                return View(vendobj);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(Courier vendobj)
        {
            //CourierVM.Courier.Id = 0;
            if (ModelState.IsValid)
            {
                if (vendobj.Id == 0)
                {
                    await _unitOfWork.Courier.AddAsync(vendobj);
                }
                else
                {
                    await _unitOfWork.Courier.UpdateAsync(vendobj);
                }

                await _unitOfWork.SaveAsync();
                return RedirectToAction("Index", "Courier");
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
            Courier prodfromdb = await _unitOfWork.Courier.GetAsync(u => u.Id == id);
            if (prodfromdb == null)
            {
                return NotFound();
            }
            return View(prodfromdb);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DelectPost(int id)
        {
            Courier prodCategory = await _unitOfWork.Courier.GetAsync(u => u.Id == id);
            if (prodCategory == null)
            {
                return NotFound();
            }
            await _unitOfWork.Courier.RemoveAsync(prodCategory);
            await _unitOfWork.SaveAsync();
            return RedirectToAction("Index", "Courier");
        }





    }
}

