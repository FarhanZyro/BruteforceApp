using System;
using System.Security.Cryptography;
using System.Text;

namespace BruteforceApp1.Core
{
    // Converts a password into a SHA256 hash. Cannot be reversed.
    public static class PasswordHasher
    {
        // This salt is added before every password before hashing
        private const string STATIC_SALT = "VU_SALT_2024";

        public static string Hash(string plainTextPassword)
        {
            // Combine salt + password
            string saltedPassword = STATIC_SALT + plainTextPassword;

            // Convert to bytes
            byte[] passwordBytes = Encoding.UTF8.GetBytes(saltedPassword);

            // Run SHA256
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                return ConvertBytesToHexString(hashBytes);
            }
        }

        public static bool Verify(string candidate, string storedHash)
        {
            string candidateHash = Hash(candidate);
            return string.Equals(candidateHash, storedHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string ConvertBytesToHexString(byte[] bytes)
        {
            StringBuilder hexBuilder = new StringBuilder();
            foreach (byte b in bytes)
                hexBuilder.AppendFormat("{0:x2}", b);
            return hexBuilder.ToString();
        }
    }
}