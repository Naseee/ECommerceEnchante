using ECommerceApp.Models;
using System.Linq.Expressions;

namespace ECommerceApp.Repository.IRepository
{
    public interface IProductRepository:IRepository<Product>
    {

        void Update(Product product);
        public bool ProductExists(string productName);

    }
}
