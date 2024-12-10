using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Repository;
using ECommerceApp.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stripe;
using PayPalCheckoutSdk;
using OfficeOpenXml;
using ECommerceApp.Settings;
using ECommerceApp.Services.IServices;
using ECommerceApp.Services;
using ECommerceApp.Data.DbInitialiser;



var builder = WebApplication.CreateBuilder(args);
var licenseKey = "IRONSUITE.NASIYA864.GMAIL.COM.10372-8B846A5FC6-H4FPE7VIO7PHLT-TWBU3OL2PCPO-6J63SIU4VUUT-IUENGTK6IICN-BRCVMUJSEQUV-OY2MK7Q437TF-C3Z4T6-TNQ3RD3ZJV6OEA-DEPLOYMENT.TRIAL-RHE2KY.TRIAL.EXPIRES.09.JAN.2025";
IronPdf.License.LicenseKey = licenseKey;
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Add services to the container.
builder.Services.AddMemoryCache();

//builder.Services.AddAuthentication(options =>
//{
//   options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//   options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
//})
//.AddCookie()
//.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
//{
//  options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
//options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
//});


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDbInitialiser, DbInitialiser>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOfferDiscountService, OfferDiscountService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOrderProcessingService, OrderProcessingService>();
builder.Services.Configure<StripeService>(builder.Configuration.GetSection("AppSettings"));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath= $"/Identity/Account/Logout";
    options.AccessDeniedPath= $"/Identity/Account/AccessDenied";
    
});



builder.Services.AddIdentity<ApplicationUser,IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSingleton(x => new PaypalServices(builder.Configuration["PayPalSettings:ClientId"],
    builder.Configuration["PayPalSettings:Secret"],
   
    builder.Configuration["PayPalSettings:Mode"]));
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddRazorPages();
builder.Services.AddScoped<StripeService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseStaticFiles();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
SeedDatabase();
app.MapRazorPages();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitialiser = scope.ServiceProvider.GetRequiredService<IDbInitialiser>();
        dbInitialiser.Initialize();
    }
}