using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models
{
    public class WalletTransaction
    {
        [Key]
        public int TransactionId { get; set; }
        public int WalletId { get; set; }
        [ForeignKey("WalletId")]
        [ValidateNever]
        public Wallet Wallet { get; set; }
        public decimal Amount { get; set; }
        [Required]
        public DateTime TransactionDate { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
