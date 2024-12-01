using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;
using System.Linq.Expressions;

namespace ECommerceApp.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }


        public void Update(OrderHeader orderHeader)
        {
            _db.OrderHeaders.Update(orderHeader);
        }
        public void UpdateStatus(int Id, string orderStatus, string? paymentStatus = null)
        {

            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == Id);
            if (orderFromDb != null)
            {

                orderFromDb.OrderStatus = orderStatus;

                orderFromDb.PaymentStatus = paymentStatus;
                
            }
        }
        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var orderHeader = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (orderHeader != null)
            {
                orderHeader.sessionId = sessionId;
                orderHeader.paymentIntentId = paymentIntentId;
                _db.SaveChanges();
            }
        }
    }
}
