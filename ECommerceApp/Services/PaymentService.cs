﻿using ECommerceApp.Models;
using ECommerceApp.Models.ViewModels;
using ECommerceApp.Repository.IRepository;
using ECommerceApp.Services.IServices;
using ECommerceApp.Settings;
using ECommerceApp.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using Stripe;

namespace ECommerceApp.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PaypalServices _paypalServices;
        private string domain { get; set; }
        private readonly ILogger<PaymentService> _logger;
        
        public PaymentService(IUnitOfWork unitOfWork, PaypalServices paypalServices, IOptions<AppSettings> appSettings, ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _paypalServices = paypalServices;
            _logger = logger;
            // domain = appSettings.Value.Domain;
            
        }

        public async Task<IActionResult> ProcessPaymentAsync(OrderHeader orderHeader, string paymentOption, string userId, CartVM cartVM)
        {
            switch (paymentOption)
            {
                case PaymentMethods.cod:
                    return ProcessCODPayment(orderHeader);

                case PaymentMethods.wallet:
                    return ProcessWalletPayment(orderHeader, userId);

                case PaymentMethods.paypal:
                    return await ProcessPayPalPayment(orderHeader);

                case PaymentMethods.visa:
                    return ProcessVisaPayment(orderHeader, cartVM);

                default:
                    return new ContentResult { Content = "Invalid Payment Method" };
            }
        }
        private IActionResult ProcessCODPayment(OrderHeader orderHeader)
        {
            
            if (orderHeader.OrderTotal <StaticDetails.CODMaxAmount)
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusInProcess, StaticDetails.PaymentStatusInProgress);
                orderHeader.paymentMethod = PaymentMethods.cod;

                _unitOfWork.OrderHeader.Update(orderHeader);
                _unitOfWork.Save();

                return new OkResult(); 
            }
            _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusFailed, StaticDetails.PaymentStatusFailed);
            
            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();

            return new ContentResult { Content = "COD not available for orders above $100" };
        }
        private IActionResult ProcessWalletPayment(OrderHeader orderHeader, string userId)
        {
            var wallet = _unitOfWork.Wallet.Get(w => w.UserId == userId);

            if (wallet.balance >= (decimal)orderHeader.OrderTotal)
            {
                wallet.balance -= (decimal)orderHeader.OrderTotal;

                WalletTransaction transaction = new()
                {
                    WalletId = wallet.WalletId,
                    Amount = (decimal)orderHeader.OrderTotal,
                    TransactionDate = DateTime.Now,
                    Description = "Order Payment",
                };

                _unitOfWork.Transaction.Add(transaction);
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusInProcess, StaticDetails.PaymentStatusInProgress);
                orderHeader.paymentMethod = PaymentMethods.wallet;

                _unitOfWork.OrderHeader.Update(orderHeader);
                _unitOfWork.Save();

                return new OkResult(); 
            }
            _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusFailed, StaticDetails.PaymentStatusFailed);

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            return new ContentResult { Content = "Insufficient Wallet Balance" };
        }

        private async Task<IActionResult> ProcessPayPalPayment(OrderHeader orderHeader)
        {

            string domain = "https://ecommerceapp-dbd3gba2hea5cdbj.australiaeast-01.azurewebsites.net/";
            string successUrl = $"{domain}{_paypalServices.SuccessPath}{orderHeader.Id}";
            string cancelUrl = domain + _paypalServices.CancelPath;

            try
            {
                string approvalUrl = await _paypalServices.CreateOrder((decimal)orderHeader.OrderTotal, "USD", successUrl, cancelUrl);
                orderHeader.PayPalOrderId = approvalUrl.Split("token=").Last();  
                orderHeader.paymentMethod = PaymentMethods.paypal;
                _unitOfWork.OrderHeader.Update(orderHeader);
                _unitOfWork.Save();
                return new RedirectResult(approvalUrl);

            }
            catch (Exception ex)
            {
                _logger.LogError($"PayPal payment failed: {ex.Message}");
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusFailed, StaticDetails.PaymentStatusFailed);
                
                _unitOfWork.OrderHeader.Update(orderHeader);
                _unitOfWork.Save();

                return new ContentResult { Content = "PayPal payment failed" };
            }
        }

        private IActionResult ProcessVisaPayment(OrderHeader orderHeader, CartVM cartVM)
        {
           
            var options = new SessionCreateOptions
            {
                //SuccessUrl = $"{domain}{_appSettings.Payment.VisaSuccessPath}{orderHeader.Id}",
               // CancelUrl = domain + _appSettings.Payment.VisaCancelPath,
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment"
            };

            foreach (var item in cartVM.cartList)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
                        Currency = "USD",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        }
                    },
                    Quantity = item.Quantity,
                });
            }

            var service = new SessionService();
            var session = service.Create(options);

            if (session.PaymentStatus == "paid")
            {
                orderHeader.paymentMethod = PaymentMethods.visa;
                _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusInProcess, StaticDetails.PaymentStatusInProgress);
                
                _unitOfWork.Save();

                return new RedirectResult(session.Url);
            }

            _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusFailed, StaticDetails.PaymentStatusFailed);
            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();

            return new ContentResult { Content = "Visa payment failed" };
        }
    }

}
