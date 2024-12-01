using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ECommerceApp.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ECommerceApp.Models
{
    public class CategoryOffer
    {

        [Key]
        public int Id { get; set; }
        public int OfferId { get; set; }
        [ValidateNever]
        [ForeignKey("OfferId")]
        public Offer Offer { get; set; }
        public int CategoryId { get; set; }
        [ValidateNever]
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
       
    }
}
