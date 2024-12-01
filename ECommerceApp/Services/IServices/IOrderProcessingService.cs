using ECommerceApp.Models;
using ECommerceApp.ViewModels.Models;

namespace ECommerceApp.Services.IServices
{
    public interface IOrderProcessingService
    {
        Task<bool> CancelApprovedOrder(OrderHeader orderHeader, OrderDetail orderDetail, int quantity);
    }
}
