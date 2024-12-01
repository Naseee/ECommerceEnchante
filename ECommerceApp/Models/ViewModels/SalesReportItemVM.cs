namespace ECommerceApp.Models
{
    public class SalesReportItemVM
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double Discount { get; set; }
        public double TotalPrice { get; set; }
        public int TotalQuantitySold { get; set; }
    }
}
