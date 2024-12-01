using ECommerceApp.Data.Migrations;
using ECommerceApp.Models;
using ECommerceApp.Models.ViewModels;
using ECommerceApp.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;

namespace ECommerceApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class WishListController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        [BindProperty]
        public CartVM cartVM { get; set; }
        public WishListController(IUnitOfWork unitOfWork,UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
           var wishLists = _unitOfWork.WishList.GetAll(u => u.UserId == userId&&u.IsActive!=false, includeProperties: "Product,Product.ProductImages");
            return View(wishLists);
        }
       
       
        public IActionResult Delete(int Id)
        {
            var wishListFromDB = _unitOfWork.WishList.Get(u => u.WishListId == Id);
            wishListFromDB.IsActive = false;
            _unitOfWork.WishList.Update(wishListFromDB);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
    }
}
