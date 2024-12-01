using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Models.ViewModels;
using ECommerceApp.Repository.IRepository;
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
             .Select(o => new SalesReportVM
             {
                 OrderId = o.Id,
                 OrderDate = o.OrderDate,
                 CustomerName = o.Name,
                 TotalAmount = o.OrderTotal,
                 TotalDiscountedAmount=o.DiscountedTotal,
                 OrderStatus=o.OrderStatus,
                 Items = o.OrderDetails.Select(oi => new SalesReportItemVM
                 {
                     ProductName = oi.Product.Name,
                     Quantity = oi.Quantity,
                     UnitPrice=oi.Product.Price,
                     Discount = oi.Product.DiscountedPrice > 0
                      ? oi.Product.Price -  (double)oi.Product.DiscountedPrice
                         : 0,

                     TotalPrice = oi.Quantity * (oi.Product.DiscountedPrice > 0
                      ?  (double)oi.Product.DiscountedPrice
                       : oi.Price)
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
