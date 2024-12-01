using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;
using System.Linq.Expressions;
using System.Linq;

namespace ECommerceApp.Repository
{
    public class CategoryRepository : Repository<Category>,ICategoryRepository
    {
        private ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

       

        public void Update(Category category)
        {
            _db.Categories.Update(category);
        }
        public bool CategoryExists(string categoryName)
        {
            return _db.Categories.Any(c => c.Name == categoryName);
        }
    }
}
