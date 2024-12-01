using ECommerceApp.Models;

namespace ECommerceApp.Repository.IRepository
{
    public interface IProductOfferRepository:IRepository<ProductOffer>
    {
        void Update(ProductOffer offer);
    }
}
