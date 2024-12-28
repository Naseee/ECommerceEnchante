using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Models.ViewModels;
using ECommerceApp.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ECommerceApp.Repository
{
    public class SalesReportRepository : ISalesReportRepository
    {
        private ApplicationDbContext _db;
        public SalesReportRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public IEnumerable<SalesReportVM> GetSalesReport(DateTime? startDate, DateTime? endDate)
        {
            IEnumerable<SalesReportVM> salesReport = _db.OrderHeaders
    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
    .Include(o => o.OrderDetails) 
    .OrderByDescending(o => o.Id)
    .Select(o => new SalesReportVM
    {
        OrderId = o.Id,
        OrderDate = o.OrderDate,
        CustomerName = o.Name,
        ShippingCharge=(double)o.ShippingCharge >0? (double)o.ShippingCharge : 0,
        TotalAmount = o.OrderTotal,
        TotalDiscountedAmount = o.DiscountedTotal < o.OrderTotal ? o.DiscountedTotal : o.OrderTotal,
        CouponDiscount = (double)o.CouponDiscount > 0 ? (double)o.CouponDiscount : 0,
        OrderStatus = o.OrderStatus,
        Items = o.OrderDetails.Select(oi => new SalesReportItemVM
        {
            ProductName = oi.Product.Name,
            Quantity = oi.Quantity,
            UnitPrice = oi.Product.Price,
            Discount = oi.Product.Price - oi.Price,
            TotalPrice = oi.Quantity * oi.Price
        }).ToList()
    }).ToList();
            return salesReport;
        }

        

        public IEnumerable<SalesReportVM> GetTotalSalesReport()
        {
            IEnumerable<SalesReportVM> salesReport = _db.OrderHeaders
             
             .Select(o => new SalesReportVM
             {
                 OrderId = o.Id,
                 OrderDate = o.OrderDate,
                 CustomerName = o.Name,
                 TotalAmount = o.OrderTotal,
                 Items = o.OrderDetails.Select(oi => new SalesReportItemVM
                 {
                     ProductName = oi.Product.Name,
                     Quantity = oi.Quantity,
                     UnitPrice = oi.Product.Price,
                     Discount = oi.Product.DiscountedPrice.HasValue && oi.Product.DiscountedPrice > 0
                      ? oi.Product.Price - (double)oi.Product.DiscountedPrice
                         : oi.Product.Price,

                     TotalPrice = oi.Quantity * (oi.Product.DiscountedPrice > 0
                      ? (double)oi.Product.DiscountedPrice
                       : oi.Price)
                 }).ToList()
             }).ToList();

            return salesReport;
        }

       
    }
}
