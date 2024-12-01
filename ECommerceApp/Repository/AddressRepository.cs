using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;

namespace ECommerceApp.Repository
{
    public class AddressRepository: Repository<Address>,IAddressrepository
    {
        private ApplicationDbContext _db;
        public AddressRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }



        public void Update(Address address)
        {
            _db.Addresses.Update(address);
        }
       
    }
}
