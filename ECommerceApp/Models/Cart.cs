using ECommerceApp.Data;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models
{
    public class Cart
    {
        [Key]

        public int CartId { get; set; }
        [ValidateNever]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser User { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
       
        public  Product Product { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [NotMapped]
        public double Price { get;  set; }
        //public IEnumerable<Offer>? Offers { get; set; }
    }
}
