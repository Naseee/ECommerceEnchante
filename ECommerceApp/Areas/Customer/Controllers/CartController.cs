using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Stripe.Checkout;
using ECommerceApp.Repository.IRepository;
using ECommerceApp.Models;
using ECommerceApp.Models.ViewModels;
using ECommerceApp.Utility;
using Microsoft.IdentityModel.Tokens;
using ECommerceApp.ViewModels.Models;
using Stripe;
using ECommerceApp.Data.Migrations;
using ECommerceApp.Repository;
using Stripe.Climate;
using static Microsoft.IO.RecyclableMemoryStreamManager;
using ECommerceApp.Data;
using ECommerceApp.Settings;
using ECommerceApp.Services.IServices;
using ECommerceApp.Services;

namespace ECommerceApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles ="Customer")]
    public class CartController : Controller
    {
        private readonly ILogger<CartController> _logger;
        private readonly UserManager<ApplicationUser>  _userManager;
        private readonly IOfferDiscountService _discountService;
        private readonly IPaymentService _paymentService;
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public CartVM cartVM { get; set; }
        public OrderHeader orderHeader { get; set; }
		public OrderDetail orderDetail { get; set; }
        private readonly PaypalServices _paypalServices;
        
		public CartController(IUnitOfWork unitOfWork,PaypalServices paypalServices
            ,UserManager<ApplicationUser> userManager,IPaymentService paymentService,
            ILogger<CartController> logger,IOfferDiscountService discountService)
        {
            _paypalServices = paypalServices;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _discountService = discountService;
            _paymentService = paymentService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            cartVM = new CartVM()
            {
                cartList = _unitOfWork.Cart.GetAll(u => u.UserId == userId, includeProperties: "Product,Product.ProductImages,Product.ProductOffers,Product.ProductOffers.Offer,Product.Category.CategoryOffers.Offer"),
                orderHeader=new OrderHeader()
            };
            IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll(null);
            foreach (var cart in cartVM.cartList)
            {
                cart.Product.ProductImages = productImages.Where(u=> u.ProductId==cart.Product.Id).ToList();
                var result = _discountService.GetDiscountedPriceAndOffer(cart.Product);
                cart.Price = result.DiscountedPrice;
                cartVM.orderHeader.OrderTotal += cart.Quantity * cart.Product.Price;
                cartVM.orderHeader.DiscountedTotal += cart.Quantity * cart.Price;

            }
            

            return View(cartVM);
        }
        public IActionResult Increment(int Id)
        {
            var cartfromDB = _unitOfWork.Cart.Get(u => u.CartId == Id);
            if (cartfromDB.Quantity <= 5)
            {
                cartfromDB.Quantity += 1;

                _unitOfWork.Cart.Update(cartfromDB);
                _unitOfWork.Save();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Decrement(int Id)
        {
            var cartfromDB = _unitOfWork.Cart.Get(u => u.CartId == Id);
            if (cartfromDB.Quantity <= 1)
            {
                _unitOfWork.Cart.Remove(cartfromDB);
                _unitOfWork.Save();
            }
            else
            {
                cartfromDB.Quantity -= 1;
                _unitOfWork.Cart.Update(cartfromDB);
                _unitOfWork.Save();
            }
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int Id)
        {
            var cartfromDB = _unitOfWork.Cart.Get(u => u.CartId == Id);
          
            _unitOfWork.Cart.Remove(cartfromDB);

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult OrderSummary()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            cartVM = new CartVM()
            {
                cartList = _unitOfWork.Cart.GetAll(u => u.UserId == userId, includeProperties: "Product,Product.Category.CategoryOffers.Offer,Product.ProductOffers.Offer"),
                orderHeader = new OrderHeader(),
                Address = _unitOfWork.Address.GetAll(u => u.UserId == userId&u.IsActive!=false)
		    };

            
            cartVM.orderHeader.AUser = _userManager.FindByIdAsync(userId).Result;
            cartVM.orderHeader.Name = cartVM.orderHeader.AUser.Name;
            
            

            foreach (var cart in cartVM.cartList)
            {
                var result = _discountService.GetDiscountedPriceAndOffer(cart.Product);
                cart.Price = result.DiscountedPrice;
                cartVM.orderHeader.OrderTotal += cart.Quantity * cart.Product.Price;
                cartVM.orderHeader.DiscountedTotal += cart.Quantity * cart.Price;
                
            }

            var coupon = _unitOfWork.Coupon
              .GetAll(u => u.IsActive!=false && u.EndDate > DateTime.UtcNow&& u.StartDate<DateTime.UtcNow)
                .OrderByDescending(c => c.DiscountPercentage)
                       .FirstOrDefault();

            cartVM.Coupon = coupon;

            return View(cartVM);
        }
        [HttpPost]
        [ActionName("OrderSummary")]
        public async Task<IActionResult> OrderSummaryPostAsync(int SelectedAddressId,string paymentOption,string? CouponCode)
        {
           
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
         

            cartVM.cartList = _unitOfWork.Cart.GetAll(u => u.UserId == userId, includeProperties: "Product,Product.Category.CategoryOffers.Offer,Product.ProductOffers.Offer");
                cartVM.orderHeader.OrderDate = DateTime.Now;
                cartVM.orderHeader.UserId = userId;
                
                var user = _userManager.FindByIdAsync(userId).Result;
                cartVM.orderHeader.AUser = user;

            foreach (var cart in cartVM.cartList)
            {
                var result = _discountService.GetDiscountedPriceAndOffer(cart.Product);
                cart.Price = result.DiscountedPrice;

                cartVM.orderHeader.OrderTotal += cart.Quantity * cart.Product.Price;
                
                cartVM.orderHeader.DiscountedTotal += cart.Quantity * cart.Price;

            }
            if (!string.IsNullOrEmpty(CouponCode))
            {
                if(cartVM.orderHeader.DiscountedTotal>0)
                {
                    cartVM.orderHeader.DiscountedTotal = _discountService.ApplyCouponDiscount(cartVM.orderHeader.DiscountedTotal, CouponCode);
                }
                else
                {
                    cartVM.orderHeader.DiscountedTotal = _discountService.ApplyCouponDiscount(cartVM.orderHeader.OrderTotal, CouponCode);
                }
                cartVM.orderHeader.CouponDiscount = cartVM.orderHeader.OrderTotal - cartVM.orderHeader.DiscountedTotal;
            }

            if (cartVM.orderHeader.OrderTotal < 100)
            {
                cartVM.orderHeader.ShippingCharge = 10;
                cartVM.orderHeader.OrderTotal = cartVM.orderHeader.OrderTotal+ (double)cartVM.orderHeader.ShippingCharge;
                cartVM.orderHeader.DiscountedTotal = cartVM.orderHeader.DiscountedTotal + (double)cartVM.orderHeader.ShippingCharge;
            }
            cartVM.orderHeader.PaymentStatus = StaticDetails.PaymentStatusPending;
                cartVM.orderHeader.OrderStatus = StaticDetails.StatusPending;
                cartVM.orderHeader.AddressId = SelectedAddressId;
                
                var selectedaddress = _unitOfWork.Address.Get(u => u.AddressId == SelectedAddressId && u.UserId == userId,includeProperties:"User");

            cartVM.orderHeader.Address = selectedaddress;
            _unitOfWork.OrderHeader.Add(cartVM.orderHeader);
                _unitOfWork.Save();

                foreach (var cart in cartVM.cartList)
                {
                    OrderDetail orderDetail = new()
                    {
                        ProductId = cart.ProductId,
                        OrderHeaderId = cartVM.orderHeader.Id,
                        Price = cart.Price,
                        Quantity = cart.Quantity
                    };
                    _unitOfWork.OrderDetail.Add(orderDetail);
                }
            _unitOfWork.Save();

            var paymentResult = await _paymentService.ProcessPaymentAsync(cartVM.orderHeader, paymentOption, userId, cartVM);

            if (paymentResult is ContentResult contentResult)
            {
                ViewBag.message = contentResult.Content;
                return View(cartVM);
            }
            if (paymentResult is RedirectResult redirectResult)
            {
                return redirectResult; 
            }
            return RedirectToAction("OrderConfirmation", "Cart", new { area = "Customer", id = cartVM.orderHeader.Id });

        }

        public async Task<IActionResult> OrderConfirmation(int id)
         {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "OrderDetails,OrderDetails.Product");
            switch(orderHeader.paymentMethod)
            {
                case PaymentMethods.cod:_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusApproved, StaticDetails.PaymentStatusPending);
                    break;
                case PaymentMethods.wallet:
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusApproved, StaticDetails.PaymentStatusApproved);
                    break;
                case PaymentMethods.paypal:
                    
                        try
                        {
                            bool isCaptured = await _paypalServices.CaptureOrder(orderHeader.PayPalOrderId);

                            if (isCaptured)
                            {
                                orderHeader.PaymentStatus = "Approved";
                                orderHeader.OrderStatus = "Approved";
                                _unitOfWork.OrderHeader.Update(orderHeader);
                                _unitOfWork.Save();

                            }
                            else
                            {
                                return Content("Payment capture failed.");
                            }
                        }
                        catch (Exception ex)
                        {
                            return Content($"Error: {ex.Message}");
                        }
                    
                    break;
                    
                case PaymentMethods.visa:
             
                    var service = new SessionService();
                    Session session = service.Get(orderHeader.sessionId);
                    if (session.PaymentStatus.ToLower() == "paid")
                    {
                        _unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                        _unitOfWork.OrderHeader.UpdateStatus(id, StaticDetails.StatusApproved, StaticDetails.PaymentStatusApproved);
                        
                    }
                    break;
                default:
                    return BadRequest("Invalid payment method.");
            }
            if (orderHeader.OrderStatus == StaticDetails.StatusApproved)
            {
                _unitOfWork.OrderHeader.Update(orderHeader);
                List<Cart> carts = _unitOfWork.Cart.GetAll(u => u.UserId == orderHeader.UserId).ToList();
                _unitOfWork.Cart.RemoveRange(carts);
                _unitOfWork.Save();
            }
            return View(id);
        }
}
}
