using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ECommerceApp.Models.ViewModels
{
    public class OfferVM
    {
        [ValidateNever]
        public IEnumerable<Category> Categories { get; set; }
        [ValidateNever]
    public IEnumerable<Product> Products { get; set; }
    public Offer offer { get; set; }
        public List<int>? CategoryIds { get; set; }
        public List<int>? ProductIds { get; set; }
    }
}
