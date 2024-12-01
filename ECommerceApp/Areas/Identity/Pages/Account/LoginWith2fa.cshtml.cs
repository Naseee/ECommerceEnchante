// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.UI.Services;
using ECommerceApp.Models;
using ECommerceApp.Services;
using ECommerceApp.Utility;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerceApp.Areas.Identity.Pages.Account
{
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginWith2faModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _memoryCache;
        public LoginWith2faModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginWith2faModel> logger,
            IEmailSender emailSender,
            IMemoryCache memoryCache)
            
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _memoryCache = memoryCache;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Authenticator code")]
            public string TwoFactorCode { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }



            var token = OtpGenerator.GenerateOtp();
            _memoryCache.Set(user.Id, token, TimeSpan.FromMinutes(10));

            await _emailSender.SendEmailAsync(user.Email, "2FA Code",
                $"Your OTP code is {token}. It is valid for 1 minutes.");
           
            // var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);

            //  if (providers.Any(_ => _ == "Email"))
            {
             //   var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            //    await _emailSender.SendEmailAsync(user.Email, "Email 2FA Code",
           //         $"<h1>{token}</h1>");

            }
            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string action,bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }
        
            if (action == "resend")
            {
                var token = OtpGenerator.GenerateOtp();
                _memoryCache.Set(user.Id, token, TimeSpan.FromMinutes(1));

                await _emailSender.SendEmailAsync(user.Email, "2FA Code",
                    $"Your OTP code is {token}. It is valid for 1 minutes.");

                _logger.LogInformation("Resent OTP code to user with ID '{UserId}'.", user.Id);
                TempData["Message"] = "A new OTP code has been sent to your email.";
                return Page();
            }


            if (_memoryCache.TryGetValue(user.Id, out string cachedOtp) && cachedOtp == Input.TwoFactorCode)
            {

                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);


                await _signInManager.SignInAsync(user, isPersistent: false);
                _memoryCache.Remove(user.Id);
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }

            ModelState.AddModelError(string.Empty, "Invalid OTP.");
           

            var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorSignInAsync("Email", authenticatorCode, rememberMe, Input.RememberMachine);

            var userId = await _userManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut||user.IsBlocked)

            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return Page();
            }
        }
    }
}
