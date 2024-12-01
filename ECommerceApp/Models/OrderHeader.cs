using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser AUser { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ShippingDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public double OrderTotal { get; set; }
        public double DiscountedTotal { get; set; }
        public double? CouponDiscount { get; set; }
        public double? ShippingCharge { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }
        public string? sessionId { get; set; }
        
        public string? paymentMethod { get; set; }
        public string? PayPalOrderId { get; set; }
        public string? paymentIntentId { get; set; }
        public DateTime PaymentDate { get; set; }
        [Required]
        public string Name { get; set; }
        
        public int AddressId { get; set; }
        [ForeignKey("AddressId")]
        [ValidateNever]
        public Address Address { get; set; }

          public ICollection<OrderDetail> OrderDetails { get; set; }


    }
}