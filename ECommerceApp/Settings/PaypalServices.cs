using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalCheckoutSdk.Payments;
using PayPalHttp;
using System.Linq;
using System.Threading.Tasks;
using Money = PayPalCheckoutSdk.Payments.Money;

namespace ECommerceApp.Settings
{
    public class PaypalServices
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _mode;

        public PaypalServices(string clientId, string secret, string mode)
        {
            _clientId = clientId;
            _clientSecret = secret;
            _mode = mode;
        }

        private PayPalHttpClient GetPayPalClient()
        {
            PayPalEnvironment environment = _mode?.ToLower() == "sandbox"
    ? new SandboxEnvironment(_clientId.Trim(), _clientSecret.Trim())
    : new LiveEnvironment(_clientId.Trim(), _clientSecret.Trim());

            return new PayPalHttpClient(environment);
        }

        public async Task<string> CreateOrder(decimal amount, string currency, string successUrl, string cancelUrl)
        {
            try
            {
                var client = GetPayPalClient();

                var orderRequest = new OrderRequest()
                {
                    CheckoutPaymentIntent = "CAPTURE",
                    ApplicationContext = new ApplicationContext()
                    {
                        ReturnUrl = successUrl,
                        CancelUrl = cancelUrl
                    },
                    PurchaseUnits = new List<PurchaseUnitRequest>()
                    {
                        new PurchaseUnitRequest()
                        {
                            AmountWithBreakdown = new AmountWithBreakdown()
                            {
                                CurrencyCode = currency,
                                Value = amount.ToString("F2")
                            }
                        }
                    }
                };

                var request = new OrdersCreateRequest();
                request.Prefer("return=representation");
                request.RequestBody(orderRequest);

                var response = await client.Execute(request);
                var result = response.Result<Order>();
               
                return result.Links.FirstOrDefault(link => link.Rel == "approve")?.Href
                       ?? throw new Exception("Approval link not found in PayPal response.");
            }
            catch (HttpException ex)
            {
                throw new Exception($"PayPal CreateOrder error: {ex.Message}");
            }
        }

        public async Task<bool> CaptureOrder(string orderId)
        {
            try
            {
                var client = GetPayPalClient();
                var request = new OrdersCaptureRequest(orderId);
                request.RequestBody(new OrderActionRequest());

                var response = await client.Execute(request);

                return response.StatusCode == System.Net.HttpStatusCode.Created;
            }
            catch (HttpException ex)
            {
                throw new Exception($"PayPal CaptureOrder error: {ex.Message}");
            }
        }

        public async Task<RefundResponse> RefundPayment(string captureId, decimal refundAmount, string currency = "USD")
        {
            try
            {
                var client = GetPayPalClient();

                var refundRequest = new CapturesRefundRequest(captureId);
                refundRequest.RequestBody(new RefundRequest()
                {
                    Amount = new Money()
                    {
                        CurrencyCode = currency,
                        Value = refundAmount.ToString("F2")
                    }
                });

                var response = await client.Execute(refundRequest);

                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    return new RefundResponse { IsSuccess = true };
                }
                else
                {
                    return new RefundResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Refund failed with status code: {response.StatusCode}"
                    };
                }
            }
            catch (HttpException ex)
            {
                return new RefundResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"PayPal refund error: {ex.Message}"
                };
            }
        }
        public async Task<string> GetTransactionId(string orderId)
        {
            try
            {
                var client = GetPayPalClient();
                var request = new OrdersGetRequest(orderId);  // Get the order details using the Order ID
                var response = await client.Execute(request);

                // Extract Transaction ID from the captured order response
                var capturedOrder = response.Result<PayPalCheckoutSdk.Orders.Order>();
                var transactionId = capturedOrder.PurchaseUnits.FirstOrDefault()?
                    .Payments?.Captures?.FirstOrDefault()?.Id;  // Extract the transaction ID

                return transactionId ?? throw new Exception("Transaction ID not found.");
            }
            catch (HttpException ex)
            {
                throw new Exception($"Error fetching PayPal transaction ID: {ex.Message}");
            }
        }

        public class RefundResponse
        {
            public bool IsSuccess { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
