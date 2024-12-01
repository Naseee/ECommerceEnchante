using ECommerceApp.Areas.Customer.Controllers;
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

       public async Task<bool> CancelApprovedOrder(OrderHeader orderHeader, OrderDetail orderDetail, int quantity)
        {
            if (orderHeader.paymentMethod == PaymentMethods.visa)
            {

                var refundAmount = (decimal)(orderDetail.Price * quantity);
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.paymentIntentId,
                    Amount = (long)(refundAmount * 100) // Stripe expects amount in cents
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
            }

            if (orderHeader.paymentMethod == PaymentMethods.paypal)
            {
                var refundAmount = (decimal)(orderDetail.Price * quantity);
                try
                {
                    var captureId = await _paypalServices.GetTransactionId(orderHeader.PayPalOrderId);

                    var refundResponse = await _paypalServices.RefundPayment(captureId, refundAmount);
                    if (!refundResponse.IsSuccess)
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"PayPal refund failed: {ex.Message}");
                    return false;
                }
            }


            var wallet = _unitOfWork.Wallet.Get(u => u.UserId == orderHeader.UserId);
            if (wallet != null)
            {
                wallet.balance += (decimal)(orderDetail.Price * quantity);
                WalletTransaction transaction = new WalletTransaction()
                {
                    WalletId = wallet.WalletId,
                    Amount = (decimal)(orderDetail.Price * quantity)
                };
                transaction.TransactionDate = DateTime.Now;
                transaction.Description = "deposit";
                _unitOfWork.Transaction.Add(transaction);

                _unitOfWork.Wallet.Update(wallet);
            }
            return true;
        }
    }
}
