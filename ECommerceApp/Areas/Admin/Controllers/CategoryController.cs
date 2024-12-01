using ECommerceApp.Models;

using ECommerceApp.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECommerceApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class CategoryController : Controller
        
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var categories = _unitOfWork.Category.GetAll(u=> u.IsDeleted!=true,null);
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = " Category Added Successfully";
                return RedirectToAction("Index");
            }
            TempData["error"] = " Category Added Failed";
            return View();
        }

        public IActionResult Edit(int Id)
        {
            var category = _unitOfWork.Category.Get(u=> u.Id==Id,null);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
               
                return RedirectToAction("Index");
            }
            
            return View();
        }
        public IActionResult Delete(int Id)
        {
            var category = _unitOfWork.Category.Get(u => u.Id == Id,null);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost]
        public IActionResult Delete(Category category)
        {
            if (category == null)
            {
                return NotFound();

            }
            category.IsDeleted=true;
            _unitOfWork.Category.Update(category);
            _unitOfWork.Save();
            TempData["success"] = " Category Deleted Successfully";
            return RedirectToAction("Index");
        }
    }
}
