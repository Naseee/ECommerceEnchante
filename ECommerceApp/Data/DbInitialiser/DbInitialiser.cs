using ECommerceApp.Models;
using ECommerceApp.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Data.DbInitialiser
{
    public class DbInitialiser : IDbInitialiser
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        public DbInitialiser(RoleManager<IdentityRole> roleManager,UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;

        }
        public void Initialize()
        {
            try
            {
                if(_db.Database.GetPendingMigrations().Count()>0)
                {
                    _db.Database.Migrate();
                }

            }
            catch(Exception e)
            {

            }

            if (!_roleManager.RoleExistsAsync(RoleSeeder.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(RoleSeeder.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(RoleSeeder.Role_Admin)).GetAwaiter().GetResult();
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "nasiya864@gmail.com",
                    Email = "nasiya864@gmail.com",
                    Name = "nas",
                    PhoneNumber = "111222333"

                }, "Nas123@").GetAwaiter().GetResult();
                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "nasiya864@gmail.com");
                _userManager.AddToRoleAsync(user, RoleSeeder.Role_Admin).GetAwaiter().GetResult();
            }
            return;
        }
    }
}
