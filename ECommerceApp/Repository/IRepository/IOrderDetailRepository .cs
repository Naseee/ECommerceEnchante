using ECommerceApp.Models;
using System.Linq.Expressions;

namespace ECommerceApp.Repository.IRepository
{
    public interface IOrderDetailRepository:IRepository<OrderDetail>
    {
        
       
        void Update(OrderDetail orderDetail);
      

    }
}
