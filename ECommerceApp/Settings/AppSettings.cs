namespace ECommerceApp.Settings
{
    public class AppSettings
    {
        public string Domain { get; set; }
        public string DefaultCurrency { get; set; }
        public PaymentSettings Payment { get; set; }
    }
    public class PaymentSettings
    {
        public decimal CODMaxAmount { get; set; }
        public string PayPalSuccessPath { get; set; }
        public string PayPalCancelPath { get; set; }
        public string VisaSuccessPath { get; set; }
        public string VisaCancelPath { get; set; }
    }
}
