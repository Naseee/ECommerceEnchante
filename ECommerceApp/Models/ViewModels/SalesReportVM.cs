namespace ECommerceApp.Models.ViewModels
{
    public class SalesReportVM
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public string CustomerName { get; set; }
        public double TotalAmount { get; set; }
        public double TotalDiscountedAmount { get; set; }
        public string OrderStatus { get; set; }

        public List<SalesReportItemVM> Items { get; set; }
    }
}
