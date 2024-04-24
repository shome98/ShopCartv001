using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Retail_MVC.DataAccess.Repository.IRepository;
using Retail_MVC.Models;
using Retail_MVC.Utility;
using System;
using System.Threading.Tasks;

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
            try
            {
                var vendobj = await _unitOfWork.Courier.GetAllAsync();
                return View(vendobj);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(Courier vendobj)
        {
            try
            {
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
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DelectPost(int id)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
