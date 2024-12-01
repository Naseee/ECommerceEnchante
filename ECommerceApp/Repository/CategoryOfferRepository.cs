using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;
using System.Linq.Expressions;
using System.Linq;

namespace ECommerceApp.Repository
{
    public class CategoryOfferRepository : Repository<CategoryOffer>,ICategoryOfferRepository
    {
        private ApplicationDbContext _db;
        public CategoryOfferRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

       

        public void Update(CategoryOffer Offer)
        {
            _db.CategoryOffers.Update(Offer);
        }
       
    }
}
