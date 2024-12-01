using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;

namespace ECommerceApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class AddressController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public AddressController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
           
            return View();
        }

        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Create(Address address)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                address.UserId = userId;

              
                if (!ModelState.IsValid)
                {
                   
                    var errors = ModelState.Values
                                           .SelectMany(v => v.Errors)
                                           .Select(e => e.ErrorMessage)
                                           .ToList();

                    
                    return BadRequest(new { message = "Model validation failed", errors = errors });
                }

                
                _unitOfWork.Address.Add(address);
              _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while creating the address.", error = ex.Message });
            }
        }

        public IActionResult Edit(int id)
        {
            var address = _unitOfWork.Address.Get(u => u.AddressId == id,null);
            if (address == null || address.UserId != _userManager.GetUserId(User))
            {
                return NotFound();
            }
            return View(address);
        }

        [HttpPost]
       
        public async Task<IActionResult> Edit(Address address)
        {

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            address.UserId = userId;
            if (ModelState.IsValid)
            {
                
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User ID is not found.");
                }
                
               _unitOfWork.Address.Update(address);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(address);
           
        }

        public IActionResult Delete(int id)
        {
            var address = _unitOfWork.Address.Get(u => u.AddressId == id&&u.IsActive!=false,null);
            if (address == null || address.UserId != _userManager.GetUserId(User))
            {
                return NotFound();
            }
            return View(address);
        }

        [HttpPost]
        
        public async Task<IActionResult> Delete(Address address)
        {


            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is not found.");
            }
            address.UserId = userId;
            address.IsActive = false;
            _unitOfWork.Address.Update(address);
           _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }
        [HttpGet]
        public IActionResult GetAddressList()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var addresses = _unitOfWork.Address.GetAll(
    u => u.UserId == userId && u.IsActive != false,
   null
).Select(a => new
{
    a.AddressId,
    a.Street,
    a.City,
    a.State,
    a.PostalCode
}).ToList();

            return Json(new { data = addresses });
        }

    }
}
