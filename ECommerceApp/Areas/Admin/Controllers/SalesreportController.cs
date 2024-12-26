using ECommerceApp.Models.ViewModels;
using ECommerceApp.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OfficeOpenXml;
using Stripe;
using System.Globalization;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SalesreportController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompositeViewEngine _viewEngine;
        public SalesreportController(IUnitOfWork unitOfWork,ICompositeViewEngine viewEngine)
        {
            _unitOfWork = unitOfWork;
            _viewEngine = viewEngine;
        }
        public IActionResult Index(DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime startdate = startDate ?? DateTime.MinValue;
            DateTime enddate = endDate ?? DateTime.MaxValue;

            var reportData = _unitOfWork.SalesReport.GetSalesReport(startdate, enddate);

            // Total Sales & Discounts
            int totalSalesCount = reportData.Count();
            double totalSalesAmount = reportData.Sum(u => u.TotalAmount);
            double totalDiscount = reportData.Sum(u => u.TotalDiscountedAmount);

            ViewBag.Totalsales = totalSalesCount;
            ViewBag.TotalSalesAmount = totalSalesAmount;
            ViewBag.TotalDiscount = totalDiscount;

            // Date Calculations
            DateTime today = DateTime.UtcNow.Date;
            DateTime startWeekly = today.AddDays(-(int)today.DayOfWeek + 1);
            DateTime startMonthly = new DateTime(today.Year, today.Month, 1);
            DateTime startAnnual = new DateTime(today.Year, 1, 1);

           

            // Sales Data Grouping
            var totalSalesData = reportData
                .Where(s => s.OrderDate.HasValue)
                .GroupBy(s => s.OrderDate.Value.Date)
                .Select(g => new { Date = g.Key.ToString("yyyy-MM-dd"), TotalSales = g.Sum(s => s.TotalAmount) })
                .ToList();

            var weeklyData = reportData
                .Where(s => s.OrderDate >= startMonthly && s.OrderDate <= today)
                .GroupBy(s => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(s.OrderDate.Value, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                .Select(g => new { Week = "Week " + g.Key, TotalSales = g.Sum(s => s.TotalAmount) })
                .ToList();

            var monthlyData = reportData
                .Where(s => s.OrderDate >= startAnnual && s.OrderDate <= today)
                .GroupBy(s => new { Year = s.OrderDate.Value.Year, Month = s.OrderDate.Value.Month })
                .Select(g => new { Month = $"{g.Key.Year}-{g.Key.Month:D2}", TotalSales = g.Sum(s => s.TotalAmount) })
                .ToList();

            var annualData = reportData
                .Where(s => s.OrderDate >= startAnnual && s.OrderDate <= today)
                .GroupBy(s => s.OrderDate.Value.Year)
                .Select(g => new { Year = g.Key.ToString(), TotalSales = g.Sum(s => s.TotalAmount) })
                .ToList();

            ViewBag.TotalSalesData = totalSalesData;
            ViewBag.MonthlySalesData = weeklyData;
            ViewBag.AnnualSalesData = monthlyData;

            // Order Status Data
            var orderStatusData = reportData
                .GroupBy(s => s.OrderStatus)
                .Select(g => new { OrderStatus = g.Key, OrderCount = g.Count(), TotalAmount = g.Sum(s => s.TotalAmount) })
                .ToList();

            var orderStatusCounts = orderStatusData.ToDictionary(x => x.OrderStatus, x => x.OrderCount);
            ViewBag.OrderStatusLabels = Newtonsoft.Json.JsonConvert.SerializeObject(orderStatusCounts.Keys);
            ViewBag.OrderStatusValues = Newtonsoft.Json.JsonConvert.SerializeObject(orderStatusCounts.Values);

            // Best Selling Product & Category
            var bestSellingProduct = _unitOfWork.OrderDetail.GetAll(null,includeProperties: "Product")
                .GroupBy(od => od.ProductId)
                .Select(g => new { ProductName = g.FirstOrDefault().Product.Name, TotalQuantitySold = g.Sum(od => od.Quantity) })
                .OrderByDescending(p => p.TotalQuantitySold)
                .FirstOrDefault()?.ProductName;

            var bestSellingCategory = _unitOfWork.OrderDetail.GetAll(null,includeProperties: "Product.Category")
                .GroupBy(od => od.Product.CategoryId)
                .Select(g => new { CategoryName = g.FirstOrDefault().Product.Category.Name, TotalQuantitySold = g.Sum(od => od.Quantity) })
                .OrderByDescending(p => p.TotalQuantitySold)
                .FirstOrDefault()?.CategoryName;

            ViewBag.BestSellingProduct = bestSellingProduct;
            ViewBag.BestSellingCategory = bestSellingCategory;

            return View(reportData);
        }


        public IActionResult SalesReport(DateTime? startDate, DateTime? endDate ,int pageNumber = 1, int pageSize = 10)
        {
            DateTime startdate = startDate ?? DateTime.MinValue;
            DateTime enddate = endDate ?? DateTime.MaxValue;
           
            var report = _unitOfWork.SalesReport.GetSalesReport(startdate, enddate);
            int totalSales = report.Count();
           double totalSalesAmount = report.Sum(u=>u.TotalAmount);
          
           double totalDiscount =  report.Sum(u => u.TotalDiscountedAmount);
            var pagedReport = report
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToList();

            ViewBag.TotalSales = totalSales;
            ViewBag.TotalSalesAmount = totalSalesAmount;
            ViewBag.TotalDiscount = totalDiscount;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalSales / pageSize);
            
            return View(pagedReport);
        }

        public IActionResult GeneratePdf(DateTime? startDate,DateTime? endDate)
        {
            DateTime startdate = startDate ?? DateTime.MinValue;
            DateTime enddate = endDate ?? DateTime.MaxValue;

            var report = _unitOfWork.SalesReport.GetSalesReport(startdate, enddate);
            int totalsales = report.Count();
            double totalSalesAmount = report.Sum(u => u.TotalAmount);
            //double totalDiscount = report.Sum(u => u.TotalAmount) - report.Sum(u => u.TotalDiscountedAmount); ;
            ViewBag.Totalsales = totalsales;
            ViewBag.TotalSalesAmount = totalSalesAmount;
            string htmlContent = RenderViewToString("GeneratePdf", report);
            var renderer = new ChromePdfRenderer();
            var pdf = renderer.RenderHtmlAsPdf(htmlContent);
            return File(pdf.BinaryData, "application/pdf", "SalesReport.pdf");
        }
        private string RenderViewToString(string viewName, object model)
        {
            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
                if (!viewResult.Success)
                {
                    throw new InvalidOperationException($"View '{viewName}' not found.");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw, new HtmlHelperOptions()
                );

                viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();
                return sw.ToString();
            }
        }
        public IActionResult GenerateExcel(DateTime? startDate, DateTime? endDate)
        {
          
            DateTime startdate = startDate ?? DateTime.MinValue;
            DateTime enddate = endDate ?? DateTime.MaxValue;

            var report = _unitOfWork.SalesReport.GetSalesReport(startdate, enddate);
            int totalSales = report.Count();
            double totalSalesAmount = report.Sum(u => u.TotalAmount);
            double totalCouponDiscount = report.Sum(u => u.CouponDiscount);
            double totalDiscount = report.Sum(u => u.TotalDiscountedAmount);

            using (var package = new ExcelPackage())
            {
               
                var worksheet = package.Workbook.Worksheets.Add("Sales Report");

                // Add header
                worksheet.Cells[1, 1].Value = "Sales Report";
                worksheet.Cells[1, 1, 1, 7].Merge = true;
                worksheet.Cells[1, 1, 1, 7].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 1, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells[3, 1].Value = "Order ID";
                worksheet.Cells[3, 2].Value = "Date";
                worksheet.Cells[3, 3].Value = "Customer Name";
                worksheet.Cells[3, 4].Value = "Total Amount";
                worksheet.Cells[3, 5].Value = "Discounted Amount";
                worksheet.Cells[3, 6].Value = "Coupon Discount";
                worksheet.Cells[3, 7].Value = "Offer Discount";

                // Apply styling to headers
                worksheet.Cells[3, 1, 3, 7].Style.Font.Bold = true;
                worksheet.Cells[3, 1, 3, 7].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                // Populate data
                int row = 4;
                foreach (var item in report)
                {
                    worksheet.Cells[row, 1].Value = item.OrderId;
                  worksheet.Cells[row, 2].Value = item.OrderDate?.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 3].Value = item.CustomerName;
                   worksheet.Cells[row, 4].Value = item.TotalAmount;
                    worksheet.Cells[row, 5].Value = item.TotalDiscountedAmount;
                    worksheet.Cells[row, 6].Value = item.CouponDiscount;
                   worksheet.Cells[row, 7].Value = item.TotalAmount - item.TotalDiscountedAmount-item.CouponDiscount;
                    row++;
                }

                // Add totals row
                worksheet.Cells[row, 1].Value = "Totals";
                worksheet.Cells[row, 4].Value = totalSalesAmount;
                worksheet.Cells[row, 5].Value = totalDiscount;
                worksheet.Cells[row, 6].Value = totalCouponDiscount;
                worksheet.Cells[row, 7].Value = totalSalesAmount - totalDiscount - totalCouponDiscount; ;
                worksheet.Cells[row, 1, row, 7].Style.Font.Bold = true;

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Convert package to a byte array
                var excelData = package.GetAsByteArray();

                // Return the Excel file
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SalesReport.xlsx");
            }
        }


        [HttpGet]
        public JsonResult GetSalesData(DateTime startDate, DateTime endDate)
        {
            var salesData = _unitOfWork.SalesReport.GetSalesReport(startDate, endDate)
                .GroupBy(s => s.OrderDate?.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString(),
                    TotalSales = g.Sum(s => s.TotalAmount)
                })
                
                .ToList();
            
            return Json(salesData);
            
        }
    }
}
