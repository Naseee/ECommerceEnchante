using ECommerceApp.Models;
using System.Linq.Expressions;

namespace ECommerceApp.Repository.IRepository
{
    public interface IOrderHeaderRepository:IRepository<OrderHeader>
    {
        
        void Update(OrderHeader orderHeader);
        void UpdateStatus(int Id, string orderStatus, string? paymentStatus = null);

        void UpdateStripePaymentId(int Id, string sessionId, string paymentIntentId);

    }
}
