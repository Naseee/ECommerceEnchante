using ECommerceApp.Models;

namespace ECommerceApp.Repository.IRepository
{
    public interface IOfferRepository:IRepository<Offer>
    {
        void Update(Offer offer);
         bool OfferExists(string offerName);
    }
}
