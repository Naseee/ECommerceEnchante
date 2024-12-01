using ECommerceApp.Models;

namespace ECommerceApp.Repository.IRepository
{
    public interface ICategoryOfferRepository:IRepository<CategoryOffer>
    {
        void Update(CategoryOffer offer);
    }
}
