using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;

namespace ECommerceApp.Repository
{
    public class WishListRepository: Repository<WishListModel>,IWishListRepository
    {
        private ApplicationDbContext _db;
        public WishListRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }



        public void Update(WishListModel wishList)
        {
            _db.WishList.Update(wishList);
        }
       
    }
}
