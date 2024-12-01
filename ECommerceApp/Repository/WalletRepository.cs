using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;

namespace ECommerceApp.Repository
{
    public class WalletRepository:Repository<Wallet>,IWalletRepository
    {
        private ApplicationDbContext _db;
        public WalletRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        public void Update(Wallet wallet)
        {

            _db.Wallets.Update(wallet);
        }
    }
}
