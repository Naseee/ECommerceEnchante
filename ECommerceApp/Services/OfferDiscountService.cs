using ECommerceApp.Models;
using ECommerceApp.Models.ViewModels;
using ECommerceApp.Repository.IRepository;
using ECommerceApp.Services.IServices;

namespace ECommerceApp.Services
{
    public class OfferDiscountService : IOfferDiscountService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OfferDiscountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public (double DiscountedTotal, double DiscountAmount) ApplyCouponDiscount(double orderTotal, string couponCode)
        {
            var coupon = _unitOfWork.Coupon.Get(u =>
                u.Code == couponCode && u.IsActive!=false && u.EndDate > DateTime.UtcNow && u.StartDate <= DateTime.UtcNow);

            if (coupon == null) return (orderTotal,0);

            var discountAmount = orderTotal * (double)(coupon.DiscountPercentage / 100);
            
            var discountedTotal = orderTotal - discountAmount;
            return (discountedTotal, discountAmount);
        }

        
        public (double DiscountedPrice, Offer? HighestOffer) GetDiscountedPriceAndOffer(Product product)
        {
            
            var productOffer = product.ProductOffers?
                .Where(po => po.Offer != null &&
                             po.Offer.StartDate < DateTime.UtcNow &&
                             po.Offer.EndDate >= DateTime.UtcNow)
                .Select(po => po.Offer)
                .OrderByDescending(o => o.DiscountPercentage)
                .FirstOrDefault();

            
            var categoryOffer = product.Category?.CategoryOffers?
                .Where(co => co.Offer != null &&
                             co.Offer.StartDate < DateTime.UtcNow &&
                             co.Offer.EndDate >= DateTime.UtcNow)
                .Select(co => co.Offer)
                .OrderByDescending(o => o.DiscountPercentage)
                .FirstOrDefault();

            
            var highestOffer = (productOffer != null && categoryOffer != null)
                ? (productOffer.DiscountPercentage > categoryOffer.DiscountPercentage ? productOffer : categoryOffer)
                : productOffer ?? categoryOffer;
            var discountedPrice = highestOffer != null
                ? product.Price * (1 - (double)highestOffer.DiscountPercentage / 100)
                : product.Price;

            return (discountedPrice, highestOffer);
        }

    }
}
