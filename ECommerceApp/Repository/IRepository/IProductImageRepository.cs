using ECommerceApp.Models;

namespace ECommerceApp.Repository.IRepository
{
    public interface IProductImageRepository:IRepository<ProductImage>
    {
        void Update(ProductImage productImage);
    }
}
