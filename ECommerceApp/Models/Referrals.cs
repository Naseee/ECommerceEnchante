using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models
{
    public class Referrals
    {
        [Key]
        public int Id { get; set; }
        public string ReferrerId { get; set; }
        
        [ValidateNever]
        public ApplicationUser RefferedUser { get; set; }
        public string ReferreeId { get; set; }
        
        [ValidateNever]
        public ApplicationUser ReferreeUser { get; set; }

        public string ReferralCode { get; set; }
        public bool IsRewardGiven { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
