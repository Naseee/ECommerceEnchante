namespace ECommerceApp.Utility
{
    public static class CouponUtility
    {
        public static string GenerateRandomCode(int length = 10)
        {
                Random random = new Random();
                string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(characters,
             length)
                                       .Select(s => s[random.Next(s.Length)])

                                       .ToArray());
            }
           
    }
}
