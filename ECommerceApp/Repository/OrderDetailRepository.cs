using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;
using System.Linq.Expressions;
using System.Linq;

namespace ECommerceApp.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>,IOrderDetailRepository
    {
        private ApplicationDbContext _db;
        public OrderDetailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

       

        public void Update(OrderDetail orderDetail)
        {
            _db.OrderDetails.Update(orderDetail);
        }
      
    }
}
