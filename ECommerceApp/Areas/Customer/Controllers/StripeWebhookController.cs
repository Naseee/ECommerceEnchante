using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.IO;
using System.Threading.Tasks;
using ECommerceApp.Data;
using ECommerceApp.Models;
using Microsoft.Extensions.Logging;
using ECommerceApp.Repository.IRepository;
using ECommerceApp.Settings;

namespace ECommerceApp.Areas.Customer.Controllers
{
    [Route("api/stripe")]
    [ApiController]
    public class StripeWebhookController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly string _webhookSecret;

        public StripeWebhookController(Repository.IRepository.IUnitOfWork unitOfWork, ILogger<StripeWebhookController> logger, StripeSettings stripeSettings)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _webhookSecret = stripeSettings.SecretKey; // Assuming the webhook secret is stored in the same settings.
        }

    }
}
