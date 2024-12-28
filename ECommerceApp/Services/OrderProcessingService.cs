using ECommerceApp.Areas.Customer.Controllers;
using ECommerceApp.Data.Migrations;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;
using ECommerceApp.Services.IServices;
using ECommerceApp.Settings;
using ECommerceApp.Utility;

using ECommerceApp.ViewModels.Models;
using Stripe;
using System;

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
                if (order.PaymentStatus != StaticDetails.PaymentStatusApproved)
                    order.PaymentStatus = StaticDetails.PaymentStatusApproved;
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
            var remainingItems = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderHeader.Id&&u.IsActive==true);
            if (remainingItems.Count() == 1 && remainingItems.First().Quantity == quantity)
            {
                refundAmount -= orderHeader.CouponDiscount.HasValue ? (decimal)orderHeader.CouponDiscount : 0;
                refundAmount += orderHeader.ShippingCharge.HasValue? (decimal)orderHeader.ShippingCharge :0; 
            }
            

            bool refundSuccess = await ProcessRefund(orderHeader, refundAmount);
            if (!refundSuccess) return false;

            UpdateWallet(orderHeader.UserId, refundAmount);
            return true;
        }
        public async Task<bool> ReturnOrder(OrderHeader orderHeader, OrderDetail orderDetail, int quantity)
        {
            try
            {
                if (!orderDetail.IsApprovedForReturn)
                {
                    _logger.LogInformation("Return request is pending admin approval.");
                    return false;
                }
                var refundAmount = (decimal)(orderDetail.Price * quantity);
                var remainingItems = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderHeader.Id&&u.IsActive==true);
                if (remainingItems.Count() == 1 && remainingItems.First().Quantity == quantity)
                {
                    refundAmount -= orderHeader.CouponDiscount.HasValue ? (decimal)orderHeader.CouponDiscount : 0;
                    refundAmount += orderHeader.ShippingCharge.HasValue ? (decimal)orderHeader.ShippingCharge : 0;
                }

                bool refundSuccess = await ProcessRefund(orderHeader, refundAmount);
                if (!refundSuccess)
                {
                    _logger.LogError("Refund failed during return process.");
                    return false;
                }

                UpdateWallet(orderHeader.UserId, refundAmount);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Return order process failed: {ex.Message}");
                return false;
            }
        }


        private async Task<bool> ProcessRefund(OrderHeader orderHeader, decimal refundAmount)
        {
            try
            {
                orderHeader.RefundedAmount = 0;
                decimal refundableAmount = refundAmount - (decimal)(orderHeader.RefundedAmount);


                if (refundableAmount <= 0)
                {
                    _logger.LogWarning("No refundable amount left.");
                    return true;
                }

                if (orderHeader.paymentMethod == PaymentMethods.visa)
                {
                    var options = new RefundCreateOptions
                    {
                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = orderHeader.paymentIntentId,
                        Amount = (long)(refundableAmount * 100)
                    };
                    var service = new RefundService();
                    Refund refund = service.Create(options);
                }
                if (orderHeader.paymentMethod == PaymentMethods.paypal)
                {
                    var captureId = await _paypalServices.GetTransactionId(orderHeader.PayPalOrderId);
                    var refundResponse = await _paypalServices.RefundPayment(captureId, refundableAmount);
                    if (!refundResponse.IsSuccess)
                    {
                        _logger.LogError("PayPal refund failed.");
                        return false;
                    }
                }

                orderHeader.RefundedAmount += (double)refundableAmount;
                _unitOfWork.OrderHeader.Update(orderHeader);
                _unitOfWork.Save();

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
