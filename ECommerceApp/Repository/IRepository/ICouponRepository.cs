using ECommerceApp.Models;
using System.Linq.Expressions;

namespace ECommerceApp.Repository.IRepository
{
    public interface ICouponRepository:IRepository<Coupon>
    {
        
       
        void Update(Coupon coupon);
       public bool CouponExists(string couponName);

    }
}
