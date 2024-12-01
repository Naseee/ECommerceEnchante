using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;
using System.Linq.Expressions;
using System.Linq;

namespace ECommerceApp.Repository
{
    public class TransactionRepository : Repository<WalletTransaction>, ITransactionRepository
    {
        private ApplicationDbContext _db;
        public TransactionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

       

        public void Update(WalletTransaction transaction)
        {
            _db.Transactions.Update(transaction);
        }
       
    }
}
