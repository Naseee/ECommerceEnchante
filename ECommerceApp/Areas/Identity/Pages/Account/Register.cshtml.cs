// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using static System.Net.WebRequestMethods;

namespace ECommerceApp.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _memoryCache;
        private readonly ApplicationDbContext _db;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<ApplicationUser> userStore,
             
        SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IMemoryCache memoryCache,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _memoryCache = memoryCache;
            _db = db;
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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required]
            [Display(Name = "Name")]
            public string Name { get; set; }

            [Required]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            [Display(Name = "Referral Code")]
            public string ReferralCode { get; set; }
            public string? Role { get; set; }
            [ValidateNever]
            public IEnumerable<SelectListItem> RoleList { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {

            Input = new()
            {
                RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.Name = Input.Name;
                user.PhoneNumber = Input.PhoneNumber;
                user.CreatedAt = DateTime.Now;
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    
                    var wallet = new Wallet
                    {
                        UserId = user.Id, 
                        balance = 0        
                    };

                   
                    _db.Wallets.Add(wallet);
                    var referralCode =  GenerateReferralCode();
                    user.ReferralCode = referralCode;
                    _db.Users.Update(user);
                   if(!String.IsNullOrEmpty(Input.ReferralCode))
                    {
                        var referrer =_userManager.Users.FirstOrDefault(u => u.ReferralCode == Input.ReferralCode);
                        if(referrer!=null)
                        {
                            var referral = new Referrals
                            {
                                ReferrerId = referrer.Id,
                                ReferreeId = user.Id,
                                ReferralCode = Input.ReferralCode,
                                IsRewardGiven = false, 
                                CreatedAt = DateTime.Now
                            };

                            _db.Referrals.Add(referral);
                            var referrerWallet = _db.Wallets.FirstOrDefault(w => w.UserId == referrer.Id);
                            if (referrerWallet != null)
                            {
                                referrerWallet.balance += 10; 
                                _db.Wallets.Update(referrerWallet);
                                WalletTransaction transaction = new WalletTransaction()
                                {
                                    WalletId = referrerWallet.WalletId,
                                    Amount =10
                                };
                                transaction.TransactionDate = DateTime.Now;
                                transaction.Description = "deposit";
                                _db.Transactions.Add(transaction);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("ReferralCode", "Invalid referral code.");
                            return Page();
                        }
                    }
                    
                    await _db.SaveChangesAsync();
                   
                    _logger.LogInformation("User created a new account with password.");
                    if(!string.IsNullOrEmpty(Input.Role))
                    {
                        await _userManager.AddToRoleAsync(user, Input.Role);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user,RoleSeeder.Role_Customer);
                    }
                    var userId = await _userManager.GetUserIdAsync(user);
                    var otp= OtpGenerator.GenerateOtp();
                    _memoryCache.Set(userId, otp, TimeSpan.FromMinutes(1)); 

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Your OTP code is {otp}. It is valid for 1 minutes.");

                    return RedirectToPage("VerifyOtp", new { userId = userId, returnUrl = returnUrl });
                    // var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    // code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    //  var callbackUrl = Url.Page(
                    //     "/Account/ConfirmEmail",
                    //   pageHandler: null,
                    // values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    //  protocol: Request.Scheme);

                    //                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //                      $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    //                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    //              {
                    //                return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    //          }
                    //        else
                    //      {
                    //        await _signInManager.SignInAsync(user, isPersistent: false);
                    //      return LocalRedirect(returnUrl);
                    // }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
        public string GenerateReferralCode()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var referralCode = new string(Enumerable.Range(0, 8) // Code length of 8 characters
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());

            return referralCode;
        }

    }
}
