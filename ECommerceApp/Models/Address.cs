using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models
{
    public class Address
    {
        [Key]
        
        public int AddressId { get; set; }
        [ValidateNever]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser User { get; set; }
        public bool? IsActive { get; set; }
        [Required(ErrorMessage = "Street is required.")]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Name can only contain letters and numbers.")]
        public string? Street { get; set; }
        [Required(ErrorMessage = "City is required.")]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Name can only contain letters and numbers.")]
        public string? City { get; set; }
        [Required(ErrorMessage = "State is required.")]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Name can only contain letters and numbers.")]
        public string? State { get; set; }
        [Required(ErrorMessage = "Postal Code is required.")]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Name can only contain letters and numbers.")]
        public string? PostalCode { get; set; }


    }
}

