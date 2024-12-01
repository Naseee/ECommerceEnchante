using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Stripe;
using System.Linq;
using ECommerceApp.ViewModels.Models;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;
using ECommerceApp.Utility;
using ECommerceApp.Repository;
using ECommerceApp.Areas.Customer.Controllers;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using ECommerceApp.Settings;
using ECommerceApp.Services.IServices;

namespace ECommerceApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly PaypalServices _paypalServices;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderController> _logger;
        private readonly UserManager<ApplicationUser> _user;
        private readonly IOrderProcessingService _orderProcessingService;
        [BindProperty]
        public OrderVM orderVM { get; set; }
        public OrderHeader orderHeader { get; set; }
        public OrderDetail orderDetail { get; set; }

        public OrderController(IUnitOfWork unitOfWork, ILogger<OrderController> logger
           , UserManager<ApplicationUser> user,IOrderProcessingService orderProcessingService,
            PaypalServices paypalServices)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _user = user;
            _paypalServices = paypalServices;
            _orderProcessingService = orderProcessingService;
        }

        public async Task<IActionResult> Index()
        {
            return View();

        }


        public IActionResult OrderDetails(int id)
        {
            try
            {
                var order = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "AUser");
                OrderVM orderVM = new()
                {
                    OrderHeader = order,
                    OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == id, includeProperties: "Product"),
                    Addresses = _unitOfWork.Address.GetAll(u => u.UserId == order.UserId)
                };


                return View("OrderDetails", orderVM);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while creating the address.", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateOrderDetails()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "Address");

            if (orderHeaderFromDb != null)
            {

                orderHeaderFromDb.ShippingDate = orderVM.OrderHeader.ShippingDate;
                orderHeaderFromDb.Name = orderVM.OrderHeader.Name; ;
                orderHeaderFromDb.Address.Street = orderVM.OrderHeader.Address.Street;
                orderHeaderFromDb.Address.City = orderVM.OrderHeader.Address.City;
                orderHeaderFromDb.Address.State = orderVM.OrderHeader.Address.State;
                orderHeaderFromDb.Address.PostalCode = orderVM.OrderHeader.Address.PostalCode;
                _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
                _unitOfWork.Save();
            }
            // var addressFromDb = orderHeaderFromDb.Address;
            // if (addressFromDb != null)
            // {

            //   addressFromDb.Street = orderVM.OrderHeader.Address.Street;
            // addressFromDb.City = orderVM.OrderHeader.Address.City;
            //addressFromDb.State = orderVM.OrderHeader.Address.State;
            //    addressFromDb.PostalCode = orderVM.OrderHeader.Address.PostalCode;

            //    _addressRepository.Update(addressFromDb);
            //    _addressRepository.Save();

            // }
            return RedirectToAction("OrderDetails", new { id = orderHeaderFromDb.Id });

        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult StartProcessing()
        {
            var orderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "Address");
            orderFromDb.OrderStatus = StaticDetails.StatusInProcess;
            _unitOfWork.OrderHeader.Update(orderFromDb);


            _unitOfWork.Save();
            return RedirectToAction("OrderDetails", new { id = orderFromDb.Id });

        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult ShipOrder()
        {
            var orderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "Address");

            orderFromDb.OrderStatus = StaticDetails.StatusShipped;
            _unitOfWork.OrderHeader.Update(orderFromDb);
            orderFromDb.ShippingDate = DateTime.Now;
            _unitOfWork.Save();

            return RedirectToAction("OrderDetails", new { id = orderFromDb.Id });

        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeliverOrder()
        {
            var orderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "Address");

            orderFromDb.OrderStatus = StaticDetails.StatusDelivered;
            _unitOfWork.OrderHeader.Update(orderFromDb);
            orderFromDb.DeliveryDate = DateTime.Now;
            _unitOfWork.Save();

            return RedirectToAction("OrderDetails", new { id = orderFromDb.Id });

        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Cancelorder(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "Address");
            var orderDetail = _unitOfWork.OrderDetail.Get(
                u => u.OrderHeaderId == orderHeader.Id && u.ProductId == orderVM.ProductId
            );

            if (orderDetail == null)
            {
                TempData["error"] = "Product not found in this order.";
                return RedirectToAction("UserOrderDetails", new { id = orderHeader.Id });
            }
            if (orderHeader.PaymentStatus == StaticDetails.PaymentStatusApproved)
            {
                // if (orderHeader.paymentMethod == PaymentMethods.visa)
                // {
                // var refundAmount = (decimal)(orderDetail.Price * orderVM.Quantity);
                //  var options = new RefundCreateOptions
                // {
                //      Reason = RefundReasons.RequestedByCustomer,
                //     PaymentIntent = orderHeader.paymentIntentId,
                //      Amount = (long)(refundAmount * 100) // Stripe expects amount in cents
                //  };
                //  var service = new RefundService();
                //   Refund refund = service.Create(options);
                //}
                bool isCancelled = await _orderProcessingService.CancelApprovedOrder(orderHeader, orderDetail, orderVM.Quantity);

                if (!isCancelled)
                {
                    TempData["error"] = "An error occurred while canceling the order. Please try again later.";
                    return RedirectToAction("UserOrderDetails", new { id = orderHeader.Id });
                }

            }
            if (orderDetail.Quantity > orderVM.Quantity)
            {
                orderDetail.Quantity -= orderVM.Quantity;
                _unitOfWork.OrderDetail.Update(orderDetail);
                orderHeader.OrderTotal -= orderDetail.Price;
            }
            else
            {
                _unitOfWork.OrderDetail.Remove(orderDetail);
                orderHeader.OrderTotal -= orderDetail.Price;
            }
            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();

            var remainingItems = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderHeader.Id);
            if (!remainingItems.Any())
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusCancelled, StaticDetails.PaymentStatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusPartiallyCancelled, StaticDetails.PaymentStatusPartiallyRefunded);

            }

            _unitOfWork.Save();



            TempData["success"] = "Order has been successfully canceled.";
            return RedirectToAction("OrderDetails", new { id = orderHeader.Id });

        }
        [HttpGet]
        public IActionResult GetOrderList(string status)
        {
            try
            {

                IEnumerable<OrderHeader> orders = _unitOfWork.OrderHeader.GetAll(null, null);

                switch (status)
                {
                    case "pending":
                        orders = orders.Where(u => u.OrderStatus == StaticDetails.StatusPending); 
                            
                        break;
                    case "inprocess":
                        orders = orders.Where(u => u.OrderStatus == StaticDetails.StatusInProcess);
                        break;
                    case "approved":
                        orders = orders.Where(u => u.OrderStatus == StaticDetails.StatusApproved);
                        break;
                    
                    case "shipped":
                        orders = orders.Where(u => u.OrderStatus == StaticDetails.StatusShipped);
                        break;
                    case "delivered":
                        orders = orders.Where(u => u.OrderStatus == StaticDetails.StatusDelivered);
                        break;
                    case "cancelled":
                        orders = orders.Where(u => u.OrderStatus == StaticDetails.StatusCancelled);
                        break;
                    case "returned":
                        orders = orders.Where(u => u.OrderStatus == StaticDetails.StatusReturned);
                        break;
                    case "refunded":
                        orders = orders.Where(u => u.OrderStatus == StaticDetails.StatusRefunded);
                        break;
                    default:
                        break;
                }
                
                return Json(new { data = orders });
            }

            catch (Exception ex)
            {

                _logger.LogError(ex, "An error occurred while retrieving the order list.");

                return Json(new { success = false, message = "An error occurred while retrieving the order list. Please try again later." });
            }

        }


    }
}