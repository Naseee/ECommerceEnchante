using ECommerceApp.Models;

using ECommerceApp.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using ECommerceApp.Models.ViewModels;
using System.Linq;
using System.Drawing.Printing;
using PayPal.v1.CustomerDisputes;
using Stripe;
using Stripe.Climate;
using ECommerceApp.Utility;
using ECommerceApp.ViewModels.Models;
using ECommerceApp.Data.Migrations;
using ECommerceApp.Settings;
using ECommerceApp.Services.IServices;
using ECommerceApp.Services;

namespace ECommerceApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOfferDiscountService _discount;
        private readonly IOrderProcessingService _orderProcessingService;
        public HomeController(ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,IOfferDiscountService discount,
            IOrderProcessingService orderProcessingService)
        {
            _logger = logger;
            _discount = discount;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _orderProcessingService = orderProcessingService;
        }

        public IActionResult Index(string searchtext, int? categoryId, decimal? minPrice, decimal? maxPrice, string sortBy)
        {
            var products = _unitOfWork.Product.GetAll(u => u.IsDeleted!=false, includeProperties: "Category,ProductImages,ProductOffers,Category.CategoryOffers");

            HomeVM homeVM = new HomeVM
            {
                Products = products,
                Categories = _unitOfWork.Category.GetAll(null),
                Offers = _unitOfWork.Offer.GetAll(null)
            };

            return View(homeVM);

        }
        public IActionResult Shop(string searchtext, int? categoryId, string sortBy, int page = 1, int pageSize = 10)
        {
            var products = _unitOfWork.Product.GetAll(u => !u.IsDeleted, includeProperties: "Category,ProductImages,Category.CategoryOffers.Offer,ProductOffers.Offer");

            if (categoryId.HasValue)
            {
                products = _unitOfWork.Product.GetAll(u => !u.IsDeleted && u.CategoryId == categoryId, includeProperties: "Category,ProductImages,Category.CategoryOffers.Offer,ProductOffers.Offer");
                switch (sortBy)
                {
                    case "price_asc":
                        products = products.OrderBy(p => p.Price).ToList();
                        break;
                    case "price_desc":
                        products = products.OrderByDescending(p => p.Price).ToList();
                        break;
                    case "name_asc":
                        products = products.OrderBy(p => p.Name.Trim().ToLower()).ToList();
                        break;
                    case "name_desc":
                        products = products.OrderByDescending(p => p.Name).ToList();
                        break;
                    default:

                        break;

                }
                if (!string.IsNullOrEmpty(searchtext))
                {
                    var searchTerm = searchtext.Trim().ToLower();
                    products = products.Where(s => s.Name.ToLower().Contains(searchTerm) || s.Description.ToLower().Contains(searchTerm)).ToList();
                }

            }
            else
            {
                switch (sortBy)
                {
                    case "price_asc":
                        products = products.OrderBy(p => p.Price).ToList();
                        break;
                    case "price_desc":
                        products = products.OrderByDescending(p => p.Price).ToList();
                        break;
                    case "name_asc":
                        products = products.OrderBy(p => p.Name.Trim().ToLower()).ToList();
                        break;
                    case "name_desc":
                        products = products.OrderByDescending(p => p.Name).ToList();
                        break;
                    default:

                        break;

                }
                if (!string.IsNullOrEmpty(searchtext))
                {
                    var searchTerm = searchtext.Trim().ToLower();
                    products = products.Where(s => s.Name.ToLower().Contains(searchTerm) || s.Description.ToLower().Contains(searchTerm)).ToList();
                }
            }

            int totalProducts = products.Count();
            var paginatedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            HomeVM homeVM = new HomeVM
            {
                Products = paginatedProducts,
                Categories = _unitOfWork.Category.GetAll(u => u.IsDeleted != true, includeProperties: "CategoryOffers"),
                Offers = _unitOfWork.Offer.GetAll(u => u.IsActive != false, includeProperties: "CategoryOffers,ProductOffers"),
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize),
                CategoryId = categoryId
            };

            return View(homeVM);
        }
        public IActionResult Details(int Id)
        {
            var product = _unitOfWork.Product.Get(u => u.Id == Id, includeProperties: "Category,ProductImages,ProductOffers,Category.CategoryOffers,Category.CategoryOffers.Offer");

            Cart cart = new()
            {
                Product = product,
                Quantity = 1,
                ProductId = Id,
            };
            var result = _discount.GetDiscountedPriceAndOffer(product);
            cart.Price = result.DiscountedPrice;
            return View(cart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(Cart cart)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            cart.UserId = userId;
            var productFromDb = _unitOfWork.Product.Get(u => u.Id == cart.ProductId, includeProperties: "ProductOffers,Category.CategoryOffers.Offer");
            if (productFromDb == null)
            {
                ModelState.AddModelError("", "Product not found.");
                return View(cart);
            }
            if (cart.Quantity > productFromDb.product_Quantity)
            {
                ModelState.AddModelError("Quantity", $"Only {productFromDb.product_Quantity} items are available in stock.");
                cart.Product = productFromDb; 
                return View(cart);
            }
            if (ModelState.IsValid)
            {
                var result = _discount.GetDiscountedPriceAndOffer(productFromDb);
                cart.Price = result.DiscountedPrice;
                Cart cartFromDb = _unitOfWork.Cart.Get(u => u.ProductId == cart.ProductId && u.UserId == userId);

                if (cartFromDb != null)
                {
                    productFromDb.product_Quantity--;
                    _unitOfWork.Product.Update(productFromDb);
                    cartFromDb.Quantity += cart.Quantity;
                    _unitOfWork.Cart.Update(cartFromDb);
                }
                else
                {
                    productFromDb.product_Quantity--;
                    _unitOfWork.Product.Update(productFromDb);
                    _unitOfWork.Cart.Add(cart);
                }
                _unitOfWork.Save();
            }
            return RedirectToAction("Shop");
        }

        [HttpPost]
        [Authorize]
        public IActionResult WishList(int Id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = _unitOfWork.Product.Get(u => u.Id == Id, includeProperties: "ProductImages");

            WishListModel wishList = new()
            {
                Product = product,
                ProductId = Id,
                UserId = userId,
                Quantity = 1
            };
            var wishlistItemFromDb = _unitOfWork.WishList.Get(u => u.ProductId == Id);
            if (wishlistItemFromDb == null)
            {
                _unitOfWork.WishList.Add(wishList);
                _unitOfWork.Save();
            }
            return RedirectToAction("Details", new { Id = wishList.ProductId });
        }


        public IActionResult Search(string searchtext)
        {
            var products = _unitOfWork.Product.GetAll(u => !u.IsDeleted, includeProperties: "Category,ProductImages,Category.CategoryOffer");

            if (!string.IsNullOrEmpty(searchtext))
            {
                var searchTerm = searchtext.Trim().ToLower();
                products = products.Where(s => s.Name.ToLower().Contains(searchTerm) || s.Description.ToLower().Contains(searchTerm)).ToList();
            }
            HomeVM homeVM = new HomeVM
            {
                Products = products,
                Categories = _unitOfWork.Category.GetAll(null),
                Offers = _unitOfWork.Offer.GetAll(null)
            };
            return View(homeVM);
        }
        [HttpGet]
        public IActionResult UserOrderHistory(string timePeriod)
        {
            IEnumerable<OrderHeader> orders;

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             orders = _unitOfWork.OrderHeader
                       .GetAll(u => u.UserId == userId, null)
                         .OrderByDescending(o => o.Id);


            DateTime now = DateTime.Now;
            switch (timePeriod)
            {
                case "lastweek":
                    orders = orders.Where(u => u.OrderDate >= now.AddDays(-7));
                    break;
                case "lastmonth":
                    orders = orders.Where(u => u.OrderDate >= now.AddMonths(-1));
                    break;
                case "lastyear":
                    orders = orders.Where(u => u.OrderDate >= now.AddYears(-1));
                    break;
                case "all":
                default:
                    
                    break;
            }

            return View(orders);
        }
        public IActionResult UserOrderDetails(int id)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var order = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "AUser");
                OrderVM orderVM = new()
                {
                    OrderHeader = order,
                    OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == id, includeProperties: "Product,Product.ProductImages"),
                    Addresses = _unitOfWork.Address.GetAll(u => u.UserId == userId)
                };


                return View(orderVM);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while creating the address.", error = ex.Message });
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CancelOrder(OrderVM orderVM)
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
                return RedirectToAction("UserOrderDetails", new { id = orderHeader.Id });
            }

                if (orderHeader.PaymentStatus == StaticDetails.PaymentStatusApproved)
                {
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

            TempData["success"] = "Product has been successfully canceled.";
            return RedirectToAction("UserOrderDetails", new { id = orderHeader.Id });
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ReturnOrder(OrderVM orderVM)
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
                return RedirectToAction("UserOrderDetails", new { id = orderHeader.Id });
            }


            if (orderHeader.PaymentStatus == StaticDetails.PaymentStatusApproved)
            {
                bool isReturned = await _orderProcessingService.ReturnOrder(orderHeader, orderDetail, orderVM.Quantity);

                if (!isReturned)
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
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusReturned, StaticDetails.PaymentStatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusPartiallyReturned, StaticDetails.PaymentStatusPartiallyRefunded);

            }

            _unitOfWork.Save();

            TempData["success"] = "Product has been successfully canceled.";
            return RedirectToAction("UserOrderDetails", new { id = orderHeader.Id });
        }
    }
}
