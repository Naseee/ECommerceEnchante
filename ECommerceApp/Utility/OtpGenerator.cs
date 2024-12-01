using System.Text;

namespace ECommerceApp.Utility
{
    public static class OtpGenerator
    {
        public static string GenerateOtp(int length = 6)
        {
            var random = new Random();
            var otp = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                otp.Append(random.Next(0, 10));
            }
            return otp.ToString();
        }
    }

}
