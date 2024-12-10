namespace ECommerceApp.Settings
{
    public class StripeService
    {
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
        public StripeService()
        {
            SuccessUrl = "Customer/Cart/OrderConfirmation";
            CancelUrl = "Customer/Cart/Index";
        }

    }
   
}
