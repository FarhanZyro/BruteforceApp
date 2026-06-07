using System;

namespace BruteforceApp1.Core
{
    // Generates a random password of length 4 or 5
    public class PasswordGenerator
    {
        // Only these 36 characters are used
        public static readonly char[] Charset =
            "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        // Brute force will try up to this length
        public const int MaxLength = 6;

        private readonly Random _random;

        public PasswordGenerator()
        {
            _random = new Random();
        }

        public string GeneratePassword()
        {
            // Length is 4 or 5 (upper bound 6 is exclusive)
            int passwordLength = _random.Next(4, 6);

            char[] passwordChars = new char[passwordLength];
            for (int i = 0; i < passwordLength; i++)
            {
                int randomIndex = _random.Next(0, Charset.Length);
                passwordChars[i] = Charset[randomIndex];
            }

            return new string(passwordChars);
        }
    }
}