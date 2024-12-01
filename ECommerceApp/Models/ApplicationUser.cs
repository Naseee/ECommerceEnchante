using ECommerceApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models
{
    public class ApplicationUser:IdentityUser
    {
        [Required]
       public string Name { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public Wallet Wallet { get; set; }
        [UniqueName]
       public string ReferralCode { get; set; }
    }
}
