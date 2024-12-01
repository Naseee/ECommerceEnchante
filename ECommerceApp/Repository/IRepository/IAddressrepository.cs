using ECommerceApp.Models;

namespace ECommerceApp.Repository.IRepository
{
    public interface IAddressrepository:IRepository<Address>
    {
        void Update(Address address);
    }
}
