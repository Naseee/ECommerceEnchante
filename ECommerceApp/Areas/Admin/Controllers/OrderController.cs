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
using ECommerceApp.Models.ViewModels;

namespace ECommerceApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderController> _logger;
        private readonly UserManager<ApplicationUser> _user;
        private readonly IOrderProcessingService _orderProcessingService;
        [BindProperty]
        public OrderVM orderVM { get; set; }
        public OrderHeader orderHeader { get; set; }
        public OrderDetail orderDetail { get; set; }

        public OrderController(IUnitOfWork unitOfWork, ILogger<OrderController> logger
           , UserManager<ApplicationUser> user,IOrderProcessingService orderProcessingService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _user = user;
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
            
            return RedirectToAction("OrderDetails", new { id = orderHeaderFromDb.Id });

        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult StartProcessing()
        {
            var success = _orderProcessingService.UpdateOrderStatus(orderVM.OrderHeader.Id, StaticDetails.StatusInProcess, DateTime.Now);

            if (!success)
            {
                TempData["error"] = "Failed to update the order status.";
                return RedirectToAction("OrderDetails", new { id = orderVM.OrderHeader.Id });
            }

            TempData["success"] = "Order has been processed successfully.";
            return RedirectToAction("OrderDetails", new { id = orderVM.OrderHeader.Id });

        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult ShipOrder()
        {
            var success = _orderProcessingService.UpdateOrderStatus(orderVM.OrderHeader.Id, StaticDetails.StatusShipped, DateTime.Now);

            if (!success)
            {
                TempData["error"] = "Failed to update the order status.";
                return RedirectToAction("OrderDetails", new { id = orderVM.OrderHeader.Id });
            }

            TempData["success"] = "Order has been shipped successfully.";
            return RedirectToAction("OrderDetails", new { id = orderVM.OrderHeader.Id });

        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeliverOrder()
        {
            var success = _orderProcessingService.UpdateOrderStatus(orderVM.OrderHeader.Id, StaticDetails.StatusDelivered, DateTime.Now);

            if (!success)
            {
                TempData["error"] = "Failed to update the order status.";
                return RedirectToAction("OrderDetails", new { id = orderVM.OrderHeader.Id });
            }

            TempData["success"] = "Order has been shipped successfully.";
            return RedirectToAction("OrderDetails", new { id = orderVM.OrderHeader.Id });

        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Cancelorder(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(
                u => u.Id == orderVM.OrderHeader.Id

            );
            var orderDetail = _unitOfWork.OrderDetail.Get(
                u => u.OrderHeaderId == orderHeader.Id && u.ProductId == orderVM.ProductId
            );

            if (orderDetail == null)
            {
                TempData["error"] = "Product not found in this order.";
                return RedirectToAction("OrderDetails", new { id = orderHeader.Id });
            }

            if (orderHeader.PaymentStatus == StaticDetails.PaymentStatusApproved || orderHeader.PaymentStatus == StaticDetails.PaymentStatusPartiallyRefunded)
            {
                bool isCancelled = await _orderProcessingService.CancelApprovedOrder(orderHeader, orderDetail, orderVM.Quantity);

                if (!isCancelled)
                {
                    TempData["error"] = "An error occurred while canceling the order. Please try again later.";
                    return RedirectToAction("OrderDetails", new { id = orderHeader.Id });
                }

            }
            var canceledOrderDetail = new OrderDetail
            {
                OrderHeaderId = orderDetail.OrderHeaderId,
                ProductId = orderDetail.ProductId,
                Quantity = orderVM.Quantity,
                Price = orderDetail.Price,
                IsActive = false
            };
            _unitOfWork.OrderDetail.Add(canceledOrderDetail);
            if (orderDetail.Quantity > orderVM.Quantity)
            {
                orderDetail.Quantity -= orderVM.Quantity;
                _unitOfWork.OrderDetail.Update(orderDetail);
            }
            else
            {
                _unitOfWork.OrderDetail.Remove(orderDetail);
            }
            _unitOfWork.Save();

            var activeItems = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderHeader.Id && u.IsActive);
            if (!activeItems.Any())
            {

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusCancelled, StaticDetails.PaymentStatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusPartiallyCancelled, StaticDetails.PaymentStatusPartiallyRefunded);

            }

            _unitOfWork.Save();

            TempData["success"] = "Product has been successfully canceled.";
            return RedirectToAction("OrderDetails", new { id = orderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Return(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(
                u => u.Id == orderVM.OrderHeader.Id

            );
            var orderDetail = _unitOfWork.OrderDetail.Get(
                u => u.OrderHeaderId == orderHeader.Id && u.ProductId == orderVM.ProductId
            );

            if (orderDetail == null)
            {
                TempData["error"] = "Product not found in this order.";
                return RedirectToAction("OrderDetails", new { id = orderHeader.Id });
            }

            if (orderHeader.PaymentStatus == StaticDetails.PaymentStatusApproved || orderHeader.PaymentStatus == StaticDetails.PaymentStatusPartiallyRefunded)
            {
                orderDetail.IsApprovedForReturn = true;
                _unitOfWork.OrderDetail.Update(orderDetail);
                _unitOfWork.Save();
            }

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