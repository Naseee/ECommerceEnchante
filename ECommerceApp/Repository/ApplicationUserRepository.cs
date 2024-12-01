using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;

namespace ECommerceApp.Repository
{
    public class ApplicationUserRepository:Repository<ApplicationUser>,IApplicationUserRepository

    {
        private ApplicationDbContext _db;
        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public bool ReferralCodeExists(string referralCode)
        {
            return _db.Referrals.Any(c => c.ReferralCode == referralCode);
        }
    }
}
