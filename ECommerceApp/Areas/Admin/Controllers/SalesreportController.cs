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
        public IActionResult Index(DateTime? startDate, DateTime? endDate)
        {
            DateTime startdate = startDate ?? DateTime.MinValue;
            DateTime enddate = endDate ?? DateTime.MaxValue;

            var report = _unitOfWork.SalesReport.GetSalesReport(startdate, enddate);
            int totalsales = report.Count();
            double totalSalesAmount = report.Sum(u => u.TotalAmount);

            double totalDiscount = report.Sum(u => u.TotalDiscountedAmount); ;
            ViewBag.Totalsales = totalsales;
            ViewBag.TotalSalesAmount = totalSalesAmount;
            ViewBag.TotalDiscount = totalDiscount;

            DateTime today = DateTime.UtcNow.Date;
            DateTime startWeekly = today.AddDays(-(int)today.DayOfWeek + 1);

            DateTime startMonthly = new DateTime(today.Year, today.Month, 1);
            DateTime startAnnual = new DateTime(DateTime.UtcNow.Year, 1, 1);
            var dailySales = _unitOfWork.SalesReport.GetSalesReport(today, enddate).Sum(u => u.TotalAmount);
            double dailyAverage = dailySales;
            var weeklySales = _unitOfWork.SalesReport.GetSalesReport(startWeekly, enddate).Sum(u => u.TotalAmount);
            double weeklyAverage = weeklySales / 7.0;
            var monthlySales = _unitOfWork.SalesReport.GetSalesReport(startMonthly, enddate).Sum(u => u.TotalAmount);
            int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            double monthlyAverage = monthlySales / daysInMonth;
            var anualSales = _unitOfWork.SalesReport.GetSalesReport(startAnnual, enddate).Sum(u => u.TotalAmount);
            int daysInYear = (enddate - startAnnual).Days + 1;
            double anualAverage = anualSales / daysInYear;
            ViewBag.DailyAverage = dailyAverage;
            ViewBag.WeeklyAverage = weeklyAverage;
            ViewBag.MonthlyAverage = monthlyAverage;
            ViewBag.AnnualAverage = anualAverage;
            
            var totalSalesData = report.GroupBy(s => s.OrderDate?.Date).Select(g => new
            {
                Date = g.Key.Value.ToString("yyyy-MM-dd"),
                TotalSales = g.Sum(s => s.TotalAmount)
            }).ToList();

           

            var weeklyData = _unitOfWork.SalesReport.GetSalesReport(startMonthly, today)
                            .GroupBy(s => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                              s.OrderDate.Value,CalendarWeekRule.FirstDay,DayOfWeek.Monday)) // Group by week number
                            .Select(g => new
                             {
                                Week = "Week " + g.Key, 
                                TotalSales = g.Sum(s => s.TotalAmount)
                             })
                            .ToList();


            var monthlyData = _unitOfWork.SalesReport.GetSalesReport(startAnnual, today)
                             .GroupBy(s => new
                               {
                                  Year = s.OrderDate.Value.Year,
                                  Month = s.OrderDate.Value.Month
                               })
                              .Select(g => new
                                {
                                  Month = $"{g.Key.Year}-{g.Key.Month.ToString("D2")}", // Format as "YYYY-MM"
                                  TotalSales = g.Sum(s => s.TotalAmount)
                                })
                              .ToList();
         var annualData = _unitOfWork.SalesReport.GetSalesReport(startAnnual, today)
                          .GroupBy(s => s.OrderDate.Value.Year) // Group by year
                          .Select(g => new
                           {
                              Year = g.Key.ToString(), // Use the year as the label
                              TotalSales = g.Sum(s => s.TotalAmount) // Sum the total sales for the year
                           })
                       .ToList();


            ViewBag.TotalSalesData = totalSalesData.Select(item => new { Date = item.Date, TotalSales = item.TotalSales });
            ViewBag.MonthlySalesData = weeklyData.Select(item => new { Week = item.Week, TotalSales = item.TotalSales });
            ViewBag.AnnualSalesData = monthlyData.Select(item => new { Month = item.Month, TotalSales = item.TotalSales });


            var orderStatusData = _unitOfWork.SalesReport.GetSalesReport(startdate, enddate)
    .GroupBy(s => s.OrderStatus)  
    .Select(g => new
    {
        OrderStatus = g.Key,
        OrderCount = g.Count(),
        TotalAmount = g.Sum(s => s.TotalAmount)
    })
    .ToList();
            var orderStatusCounts = orderStatusData.ToDictionary(x => x.OrderStatus, x => x.OrderCount);
            ViewBag.OrderStatusLabels = Newtonsoft.Json.JsonConvert.SerializeObject(orderStatusCounts.Keys);
            ViewBag.OrderStatusValues = Newtonsoft.Json.JsonConvert.SerializeObject(orderStatusCounts.Values);
            var bestSellingProduct = _unitOfWork.OrderDetail.GetAll(null,includeProperties:"Product")
            
                .GroupBy(od => od.ProductId)
            .Select(g => new
            {
            ProductId = g.Key,
            ProductName = g.FirstOrDefault().Product.Name, 
            TotalQuantitySold = g.Sum(od => od.Quantity)
            
             })
        .OrderByDescending(p => p.TotalQuantitySold) 
        .FirstOrDefault()?.ProductName;
            ViewBag.BestSellingProduct = bestSellingProduct;
            var bestSellingCategory = _unitOfWork.OrderDetail.GetAll(null, includeProperties: "Product.Category")

                .GroupBy(od => od.Product.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                CategoryName = g.FirstOrDefault().Product.Category.Name,
                TotalQuantitySold = g.Sum(od => od.Quantity)

            })
        .OrderByDescending(p => p.TotalQuantitySold)
        .FirstOrDefault()?.CategoryName;
            ViewBag.BestSellingCategory = bestSellingCategory;
            return View(report);
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
