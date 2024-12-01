using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ECommerceApp.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ECommerceApp.Models
{
    public class ProductOffer
    {
        [Key]
        public int Id { get; set; }
        public int OfferId { get; set; }
        [ValidateNever]
        [ForeignKey("OfferId")]
        public Offer Offer { get; set; }
        public int ProductId { get; set; }
        [ValidateNever]
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
       
    }
}
