using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ECommerceApp.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        [UniqueName]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Name can only contain letters and spaces.")]

        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Invalid price format")]
        public double Price { get; set; }
       
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }
        [ValidateNever]
        public ICollection<ProductImage> ProductImages { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        [Required]
        public int product_Quantity { get; set; }
        public double? DiscountedPrice { get; set; }
        [ValidateNever]
        public IEnumerable<ProductOffer>? ProductOffers { get; set; }

    }
}
