using ECommerceApp.Models;

namespace ECommerceApp.ViewModels.Models
{
    public class OrderVM
    {
        public OrderHeader OrderHeader { get; set; }
       
        public IEnumerable<OrderDetail> OrderDetail { get; set; }
        public IEnumerable<Address> Addresses { get; set; }
        public int SelectedAddressId { get; set; }
        public int ProductId { get; set; } 
        public int Quantity { get; set; }

    }

}
