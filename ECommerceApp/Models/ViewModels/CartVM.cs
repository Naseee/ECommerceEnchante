using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ECommerceApp.Models.ViewModels
{
    public class CartVM
    {
       
       public IEnumerable<Cart> cartList { get; set; }
        public OrderHeader orderHeader { get; set; }
       public IEnumerable<Address> Address { get; set; }
       
        public int SelectedAddressId { get; set; }
        public string? CouponCode { get; set; }
        public Coupon? Coupon { get; set; }
        public double? DiscountedPrice { get; set; }
    }
}
