using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;
using System.Linq.Expressions;
using System.Linq;
using ECommerceApp.Models;

namespace ECommerceApp.Repository
{
    public class CartRepository : Repository<Cart>,ICartRepository
    {
        private ApplicationDbContext _db;
        public CartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        

        public void Update(Cart cart )
        {
            _db.Cart.Update(cart);
        }
       
    }
}
