using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;

namespace ECommerceApp.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }
        public IProductImageRepository ProductImage { get; private set; }
        public IAddressrepository Address { get; private set; }
        public ICartRepository Cart { get; private set; }
        public IOrderHeaderRepository  OrderHeader { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }
        public IWishListRepository WishList { get;private set; }
        public ICouponRepository Coupon { get; private set; }
        public ISalesReportRepository SalesReport { get;private set; }
       public ICategoryOfferRepository CategoryOffer { get; private set; }
        public IOfferRepository Offer { get;private set; }
        public IProductOfferRepository ProductOffer { get; private set; }
        public IWalletRepository Wallet { get; private set; }
        public ITransactionRepository Transaction { get; private set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            Product = new ProductRepository(_db);
            ProductImage = new ProductImageRepository(_db);
            Address = new AddressRepository(_db);
            Cart = new CartRepository(_db);
            OrderHeader = new OrderHeaderRepository(_db);
            OrderDetail = new OrderDetailRepository(_db);
            WishList = new WishListRepository(_db);
            Coupon = new CouponRepository(_db);
            SalesReport = new SalesReportRepository(_db);
            CategoryOffer = new CategoryOfferRepository(_db);
            Wallet = new WalletRepository(_db);
            Transaction = new TransactionRepository(_db);
            Offer = new OfferRepository(_db);
            ProductOffer = new ProductOfferRepository(_db);
        }
       

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
