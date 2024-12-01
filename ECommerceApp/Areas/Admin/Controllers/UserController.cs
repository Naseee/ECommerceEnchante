using ECommerceApp.Models;
using ECommerceApp.Models.ViewModels;
using ECommerceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using SendGrid;


namespace ECommerceApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(UserManager<ApplicationUser> userManager
           )
        {
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Block(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsBlocked = true;
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UnBlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsBlocked = false;
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }
       
        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _userManager.Users
       .Select(u => new
       {
           u.Id,
           u.Name,
           u.Email,
           u.PhoneNumber,
           u.IsBlocked
       })
       .ToList();
            return Json(new { data = users });
        }

    }
    
}
