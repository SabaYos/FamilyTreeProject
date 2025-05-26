using System.Security.Cryptography;
using System;

namespace LoginAPI.Helpers
{
    class RandomKeyGenerator
    {
        static void Main()
        {
            var key = GenerateRandomKey();
            Console.WriteLine($"Your JWT Secret Key: {key}");
        }

        public static string GenerateRandomKey()
        {
            var key = new byte[32]; // 32 bytes = 256 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return Convert.ToBase64String(key);
        }
    }
}