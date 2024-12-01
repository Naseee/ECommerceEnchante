using ECommerceApp.Models;

namespace ECommerceApp.Services.IServices
{
    public interface IOfferDiscountService
    {
         (double DiscountedPrice, Offer? HighestOffer) GetDiscountedPriceAndOffer(Product product);
        double ApplyCouponDiscount(double orderTotal, string couponCode);
    }
}
