using ECommerceApp.Models;

namespace ECommerceApp.Repository.IRepository
{
    public interface IUnitOfWork
    {
       
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        IProductImageRepository ProductImage { get; }
        IAddressrepository Address { get; }
        ICartRepository Cart { get; }
        IOrderHeaderRepository OrderHeader { get; }
        IOrderDetailRepository OrderDetail { get; }
       IWishListRepository WishList { get; }
        ICouponRepository Coupon { get; }
        ISalesReportRepository SalesReport { get; }
        ICategoryOfferRepository CategoryOffer { get; }
        IOfferRepository Offer { get; }
        IProductOfferRepository ProductOffer { get; }
        IWalletRepository Wallet { get;  }
        ITransactionRepository Transaction { get; }
        void Save();
    }
}
