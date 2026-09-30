using System;
using System.Security.Cryptography;

namespace RobloxServer.Security
{
    /// <summary>
    /// PBKDF2-HMAC-SHA1, 1000 iterations, 128-bit salt: the ASP.NET Identity 2.x format that
    /// shipped with Visual Studio 2015. Weak by today's standards, standard in 2015.
    /// </summary>
    public static class PasswordHasher
    {
        const int SaltSize = 16;
        const int SubkeySize = 32;
        const int Iterations = 1000;

        public static string Hash(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }

            byte[] salt;
            byte[] subkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, SaltSize, Iterations))
            {
                salt = deriveBytes.Salt;
                subkey = deriveBytes.GetBytes(SubkeySize);
            }

            byte[] output = new byte[1 + SaltSize + SubkeySize];
            Buffer.BlockCopy(salt, 0, output, 1, SaltSize);
            Buffer.BlockCopy(subkey, 0, output, 1 + SaltSize, SubkeySize);
            return Convert.ToBase64String(output);
        }

        public static bool Verify(string hashedPassword, string password)
        {
            if (string.IsNullOrEmpty(hashedPassword) || password == null)
            {
                return false;
            }

            byte[] decoded;
            try
            {
                decoded = Convert.FromBase64String(hashedPassword);
            }
            catch (FormatException)
            {
                return false;
            }

            if (decoded.Length != 1 + SaltSize + SubkeySize || decoded[0] != 0x00)
            {
                return false;
            }

            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(decoded, 1, salt, 0, SaltSize);
            byte[] storedSubkey = new byte[SubkeySize];
            Buffer.BlockCopy(decoded, 1 + SaltSize, storedSubkey, 0, SubkeySize);

            byte[] generatedSubkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                generatedSubkey = deriveBytes.GetBytes(SubkeySize);
            }

            return ConstantTimeEquals(storedSubkey, generatedSubkey);
        }

        public static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }

        public static bool ConstantTimeEquals(string a, string b)
        {
            if (a == null || b == null)
            {
                return false;
            }
            return ConstantTimeEquals(System.Text.Encoding.UTF8.GetBytes(a), System.Text.Encoding.UTF8.GetBytes(b));
        }
    }
}
