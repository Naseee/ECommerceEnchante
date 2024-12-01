using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;

namespace ECommerceApp.Repository
{
    public class ProductRepository : Repository<Product>,IProductRepository
    {
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }



        public void Update(Product product)
        {
            _db.Products.Update(product);
        }
        public bool ProductExists(string productName)
        {
            return _db.Products.Any(c => c.Name == productName);
        }
    }
}
