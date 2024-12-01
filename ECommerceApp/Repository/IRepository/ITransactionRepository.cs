using ECommerceApp.Models;

namespace ECommerceApp.Repository.IRepository
{
    public interface ITransactionRepository:IRepository<WalletTransaction>
    {
        void Update(WalletTransaction transaction);
    }
}
