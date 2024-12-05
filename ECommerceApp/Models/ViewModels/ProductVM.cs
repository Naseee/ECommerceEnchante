using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ECommerceApp.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }
        [ValidateNever]
        public IEnumerable<ProductImage> ProductImages { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }
}
