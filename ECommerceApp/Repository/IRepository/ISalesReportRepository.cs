using ECommerceApp.Models.ViewModels;

namespace ECommerceApp.Repository.IRepository
{
    public interface ISalesReportRepository
    {
        public IEnumerable<SalesReportVM> GetSalesReport(DateTime? startDate, DateTime? endDate);
        public IEnumerable<SalesReportVM> GetTotalSalesReport();
    }
}
