using ECommerceApp.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Category
    {

        [Key]
        public int Id { get; set; }
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Name can only contain letters.")]
        [Required(ErrorMessage = "The Name field is required.")]
        [MaxLength(30)]
        [UniqueName]
        public string Name { get; set; }


        [MaxLength(100, ErrorMessage = "The Description field not more than 100 characters long.")]

        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
       
        [ValidateNever]
        public IEnumerable<CategoryOffer> CategoryOffers { get; set; }
    }
}
