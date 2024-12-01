using ECommerceApp.Models;

namespace ECommerceApp.Repository.IRepository
{
    public interface IWalletRepository:IRepository<Wallet>
    {
        void Update(Wallet wallet);
    }
}
