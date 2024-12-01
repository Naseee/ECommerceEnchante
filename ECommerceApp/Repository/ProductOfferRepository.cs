using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;
using System.Linq.Expressions;
using System.Linq;

namespace ECommerceApp.Repository
{
    public class ProductOfferRepository : Repository<ProductOffer>,IProductOfferRepository
    {
        private ApplicationDbContext _db;
        public ProductOfferRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

       

        public void Update(ProductOffer Offer)
        {
            _db.ProductOffers.Update(Offer);
        }
       
    }
}
