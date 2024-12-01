using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;

namespace ECommerceApp.Models
{
    public class Wallet
    {
        [Key]
        public int WalletId { get; set; }
        public decimal balance { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser User { get; set; }
        public IEnumerable<WalletTransaction> Transactions { get; set;}

    }
}
