using ECommerceApp.Areas.Customer.Controllers;
using ECommerceApp.Data.Migrations;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;
using ECommerceApp.Services.IServices;
using ECommerceApp.Settings;
using ECommerceApp.Utility;

using ECommerceApp.ViewModels.Models;
using Stripe;

namespace ECommerceApp.Services
{
    public class OrderProcessingService : IOrderProcessingService
    {
        private readonly ILogger<OrderProcessingService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PaypalServices _paypalServices;
        public OrderProcessingService(ILogger<OrderProcessingService> logger,IUnitOfWork unitOfWork,PaypalServices paypalServices)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _paypalServices = paypalServices;
        }
        public bool UpdateOrderStatus(int orderId, string newStatus, DateTime? statusDate = null)
        {
            try
            {
                var order = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "Address");
                if (order == null) return false;

                order.OrderStatus = newStatus;

                if (newStatus == StaticDetails.StatusShipped)
                    order.ShippingDate = statusDate ?? DateTime.Now;
                if (newStatus == StaticDetails.StatusDelivered)
                    order.DeliveryDate = statusDate ?? DateTime.Now;

                _unitOfWork.OrderHeader.Update(order);
                _unitOfWork.Save();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update order status for Order ID {orderId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CancelApprovedOrder(OrderHeader orderHeader, OrderDetail orderDetail, int quantity)
        {

            var refundAmount = (decimal)(orderDetail.Price * quantity);
            bool refundSuccess = await ProcessRefund(orderHeader, refundAmount);
            if (!refundSuccess) return false;

            UpdateWallet(orderHeader.UserId, refundAmount);
            return true;
        }
        public async Task<bool> ReturnOrder(OrderHeader orderHeader, OrderDetail orderDetail, int quantity)
        {
            var refundAmount = (decimal)(orderDetail.Price * quantity);
            bool refundSuccess = await ProcessRefund(orderHeader, refundAmount);
            if (!refundSuccess) return false;

            UpdateWallet(orderHeader.UserId, refundAmount);
            return true;
        }
        private async Task<bool> ProcessRefund(OrderHeader orderHeader, decimal refundAmount)
        {
            try
            {
                if (orderHeader.paymentMethod == PaymentMethods.visa)
                {
                    var options = new RefundCreateOptions
                    {
                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = orderHeader.paymentIntentId,
                        Amount = (long)(refundAmount * 100) // Stripe expects amount in cents
                    };
                    var service = new RefundService();
                    Refund refund = service.Create(options);
                }
                else if (orderHeader.paymentMethod == PaymentMethods.paypal)
                {
                    var captureId = await _paypalServices.GetTransactionId(orderHeader.PayPalOrderId);
                    var refundResponse = await _paypalServices.RefundPayment(captureId, refundAmount);
                    if (!refundResponse.IsSuccess)
                    {
                        _logger.LogError("PayPal refund failed.");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Refund processing failed: {ex.Message}");
                return false;
            }
        }

        private void UpdateWallet(string userId, decimal amount)
        {
            var wallet = _unitOfWork.Wallet.Get(u => u.UserId == userId);
            if (wallet != null)
            {
                wallet.balance += amount;
                WalletTransaction transaction = new WalletTransaction
                {
                    WalletId = wallet.WalletId,
                    Amount = amount,
                    TransactionDate = DateTime.Now,
                    Description = "Deposit"
                };
                _unitOfWork.Transaction.Add(transaction);
                _unitOfWork.Wallet.Update(wallet);
                _unitOfWork.Save();
            }
        }
    }
}
