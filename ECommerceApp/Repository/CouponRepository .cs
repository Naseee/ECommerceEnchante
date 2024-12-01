using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;
using System.Linq.Expressions;
using System.Linq;

namespace ECommerceApp.Repository
{
    public class CouponRepository : Repository<Coupon>,ICouponRepository
    {
        private ApplicationDbContext _db;
        public CouponRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

       

        public void Update(Coupon coupon)
        {
            _db.Coupons.Update(coupon);
        }
        public bool CouponExists(string couponName)
        {
           return _db.Coupons.Any(c => c.Code == couponName);
        }
    }
}
