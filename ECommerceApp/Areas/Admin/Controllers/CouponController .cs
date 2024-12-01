using ECommerceApp.Models;

using ECommerceApp.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Utility;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECommerceApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class CouponController : Controller
        
    {
        private readonly IUnitOfWork _unitOfWork;
        public CouponController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var coupons = _unitOfWork.Coupon.GetAll(null, null);
           
           // var availableCoupons = _unitOfWork.Coupon.GetAll(u => u.IsActive != false, null);
            return View(coupons);
        }

        public IActionResult Create()
        {
            var coupon = new Coupon();
           
            
            return View(coupon);
        }

        [HttpPost]
        public IActionResult Create(Coupon obj)
        {
            if (ModelState.IsValid)
            {
                
                var existingCoupon = _unitOfWork.Coupon.Get(c => c.Code == obj.Code);
                if (existingCoupon != null)
                {
                    ModelState.AddModelError("Code", "Coupon code already exists.");
                    return View(obj);
                }
                _unitOfWork.Coupon.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Coupon added successfully.";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Coupon creation failed.";
            return View(obj);
        }

        public IActionResult Edit(int Id)
        {
            var coupon = _unitOfWork.Coupon.Get(u=> u.Id==Id,null);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        [HttpPost]
        public IActionResult Edit(Coupon obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Coupon.Update(obj);
                _unitOfWork.Save();
               
                return RedirectToAction("Index");
            }
            
            return View();
        }
        public IActionResult Delete(int Id)
        {
            var coupon = _unitOfWork.Coupon.Get(u => u.Id == Id,null);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }
        [HttpPost]
        public IActionResult Delete(Coupon coupon)
        {
            if (coupon == null)
            {
                return NotFound();

            }
            coupon.IsActive=false;
            _unitOfWork.Coupon.Update(coupon);
            _unitOfWork.Save();
            TempData["success"] = " Category Deleted Successfully";
            return RedirectToAction("Index");
        }
    }
}
