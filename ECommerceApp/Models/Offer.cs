using ECommerceApp.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Models
{
    [Index(nameof(OfferName), IsUnique = true)]
    public class Offer
    {
        [Key]
        public int OfferId { get; set; }
        [Required(ErrorMessage = "Offer Name is required.")]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Name can only contain letters and numbers.")]
        
        public string OfferName { get; set; }
        [Required(ErrorMessage = " This Field is required.")]
        public decimal DiscountPercentage { get; set; }
        [Required(ErrorMessage = "This field is required.")]

        [DataType(DataType.Date, ErrorMessage = "Invalid End Date format.")]
       
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "This field is required.")]

        [DataType(DataType.Date, ErrorMessage = "Invalid End Date format.")]
        [DateAndTimeAttribute("StartDate")]
        public DateTime EndDate { get; set; }
        public bool? IsActive { get; set; }
        [ValidateNever]
        public string? OfferType { get; set; }
       
       

        // Navigation properties
        public ICollection<CategoryOffer>? CategoryOffers { get; set; }
        public ICollection<ProductOffer>? ProductOffers { get; set; }
    }
}
