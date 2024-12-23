using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;
using System.Linq.Expressions;
using System.Linq;

namespace ECommerceApp.Repository
{
    public class OfferRepository : Repository<Offer>,IOfferRepository
    {
        private ApplicationDbContext _db;
        public OfferRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

       

        public void Update(Offer Offer)
        {
            _db.Offers.Update(Offer);
        }
        public bool OfferExists(string offerName)
        {
            return _db.Offers.Any(c => c.OfferName == offerName);
        }

    }
}
