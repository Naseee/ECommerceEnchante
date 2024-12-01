using ECommerceApp.Models;
using System.Linq.Expressions;

namespace ECommerceApp.Repository.IRepository
{
    public interface ICategoryRepository:IRepository<Category>
    {
        
       
        void Update(Category category);
        public bool CategoryExists(string categoryName);

    }
}
