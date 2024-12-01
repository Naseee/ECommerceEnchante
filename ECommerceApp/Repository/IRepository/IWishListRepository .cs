using ECommerceApp.Models;

namespace ECommerceApp.Repository.IRepository
{
    public interface IWishListRepository:IRepository<WishListModel>
    {
        void Update(WishListModel wishList);
    }
}
