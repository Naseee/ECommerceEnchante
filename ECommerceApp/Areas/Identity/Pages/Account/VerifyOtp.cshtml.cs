using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using ECommerceApp.Models;
using ECommerceApp.Utility;
using ECommerceApp.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ECommerceApp.Areas.Identity.Pages.Account
{
    public class VerifyOtpModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMemoryCache _memoryCache;
        private readonly IEmailSender _emailSender;

        public VerifyOtpModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IMemoryCache memoryCache,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _memoryCache = memoryCache;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            public string Otp { get; set; }
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string userId, string returnUrl = null)
        {
            if (userId == null)
            {
                return RedirectToPage("/Index");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }
            Input = new InputModel
            {
                Email = user.Email 
            };
            ReturnUrl = returnUrl;
        
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string userId, string returnUrl = null, string action = null)
        {
            if (userId == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }
            if(action=="resend")
            {
                var otp = OtpGenerator.GenerateOtp();
                _memoryCache.Set(userId, otp, TimeSpan.FromMinutes(1));

                await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    $"Your OTP code is {otp}. It is valid for 1 minutes.");
                TempData["Message"] = "A new OTP has been sent to your email.";
                return Page();
            }
            if (_memoryCache.TryGetValue(userId, out string cachedOtp) && cachedOtp == Input.Otp)
            {
                
                    user.EmailConfirmed = true;
                user.TwoFactorEnabled = true;
                    await _userManager.UpdateAsync(user);
                
                
                await _signInManager.SignInAsync(user, isPersistent: false);
                _memoryCache.Remove(userId); 
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }

            ModelState.AddModelError(string.Empty, "Invalid OTP.");
            return Page();
        }
       
    }


}
