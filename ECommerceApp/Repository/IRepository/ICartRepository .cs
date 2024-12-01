using ECommerceApp.Models;
using ECommerceApp.Models;
using System.Linq.Expressions;

namespace ECommerceApp.Repository.IRepository
{
    public interface ICartRepository:IRepository<Cart>
    {
        
       
        void Update(Cart cart);
       

    }
}
