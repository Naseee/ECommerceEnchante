using ECommerceApp.Models;
using ECommerceApp.Utility;
using ECommerceApp.ViewModels.Models;

namespace ECommerceApp.Services.IServices
{
    public interface IOrderProcessingService
    {
        Task<bool> CancelApprovedOrder(OrderHeader orderHeader, OrderDetail orderDetail,int quantity);
        Task<bool> ReturnOrder(OrderHeader orderHeader,OrderDetail orderDetail,int quantity);
        bool UpdateOrderStatus(int orderId, string newStatus, DateTime? statusDate = null);
    }
}
