using ECommerceApp.Models;

namespace ECommerceApp.Repository.IRepository
{
    public interface IApplicationUserRepository:IRepository<ApplicationUser>
    {
        public bool ReferralCodeExists(string categoryName);
    }
}
