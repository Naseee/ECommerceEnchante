using ECommerceApp.Models;
using ECommerceApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Services.IServices
{
    public interface IPaymentService
    {
        Task<IActionResult> ProcessPaymentAsync(OrderHeader orderHeader, string paymentOption, string userId, CartVM cartVM);
    }

}
