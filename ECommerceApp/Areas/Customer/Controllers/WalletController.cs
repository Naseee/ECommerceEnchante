using ECommerceApp.Models.ViewModels;
using ECommerceApp.Models;
using ECommerceApp.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Transactions;
using PayPal.v1.Orders;
using ECommerceApp.ViewModels.Models;
using Microsoft.AspNetCore.Identity;
using ECommerceApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ECommerceApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class WalletController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public WalletVM walletVM { get; set; }
        public Wallet wallet { get; set; }

        public WalletTransaction WalletTransaction { get; set; }
        public WalletController(IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _db.ApplicationUsers.Include(u => u.Wallet).ThenInclude(u => u.Transactions).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.Wallet == null)
            {
                return NotFound("Wallet not found.");
            }

            var wallet = user.Wallet;
            var totalTransactions = wallet.Transactions.Count();
            var pagedTransactions = wallet.Transactions
                .OrderByDescending(t => t.TransactionDate) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            wallet.Transactions = pagedTransactions;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalTransactions / pageSize);
            return View(user.Wallet);
        }

        [HttpPost]
        public IActionResult Add(string userId, decimal amount)
        {
            
            var wallet = _unitOfWork.Wallet.Get(u => u.UserId == userId);
            wallet.balance += amount;
            WalletTransaction transaction = new WalletTransaction()
            {
                WalletId = wallet.WalletId,
                Amount = amount
            };
            transaction.TransactionDate = DateTime.Now;
            transaction.Description = "deposit";
            _unitOfWork.Transaction.Add(transaction);


            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Withdraw(string userId, decimal amount)
        {
            var wallet = _unitOfWork.Wallet.Get(u => u.UserId == userId);
            if (wallet == null)
            {
                NotFound();
            }
            else
            {
                wallet.balance -= amount;
                WalletTransaction transaction = new WalletTransaction()
                {
                    WalletId = wallet.WalletId,
                    Amount = amount
                };
                transaction.TransactionDate = DateTime.Now;
                transaction.Description = "Withdrawal";
                _unitOfWork.Transaction.Add(transaction);
            }

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        public IActionResult ViewTransactions(int id)
        {
            var transactions = _unitOfWork.Transaction.GetAll(u => u.TransactionId == id);
            return View(transactions);
        }
    }
}
