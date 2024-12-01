using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models
{
    public class WishListModel
    {
        [Key]
        public int WishListId { get; set; }
        [ValidateNever]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser User { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }
        public bool? IsActive { get; set; }
        [Range(1, 2, ErrorMessage = "Cannot add more than 1.")]
        public int Quantity { get; set; }
        [NotMapped]
        public double Price { get; set; }
    }



}

