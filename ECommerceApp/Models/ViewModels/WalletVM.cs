using System.Transactions;

namespace ECommerceApp.Models.ViewModels
{
    public class WalletVM
    {
        public Wallet Wallet { get; set; }
        public IEnumerable<WalletTransaction> Transactions { get; set; }
    }
}
