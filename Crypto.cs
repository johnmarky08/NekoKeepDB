using System.Security.Cryptography;
using System.Text;
using static System.Console;

namespace NekoKeepDB
{
    // AES-256 Cryptography
    public static class Crypto
    {
        // Generate a cryptographically secure random key and return Base64
        public static string GenerateMasterKeyBase64()
        {
            var key = new byte[32];
            RandomNumberGenerator.Fill(key);
            return Convert.ToBase64String(key);
        }

        // Load master key from environment or secure store (32 bytes for AES-256)
        private static byte[]? GetMasterKey()
        {
            var b64 = Environment.GetEnvironmentVariable("MASTER_KEY_B64");
            if (string.IsNullOrEmpty(b64))
            {
                WriteLine("Master key not configured.");
                return null;
            }
            var key = Convert.FromBase64String(b64);
            if (key.Length != 32)
            {
                WriteLine("Master key must be 32 bytes (base64 of 32 bytes).");
                return null;
            }

            return key;
        }

        // Combine nonce|tag|ciphertext into one byte[] for storage
        private static byte[] Combine(byte[] a, byte[] b, byte[] c)
        {
            var outb = new byte[a.Length + b.Length + c.Length];
            Buffer.BlockCopy(a, 0, outb, 0, a.Length);
            Buffer.BlockCopy(b, 0, outb, a.Length, b.Length);
            Buffer.BlockCopy(c, 0, outb, a.Length + b.Length, c.Length);
            return outb;
        }

        // Encrypt plaintext -> returns raw bytes (nonce|tag|ciphertext)
        public static byte[]? Encrypt(string plaintext)
        {
            var key = GetMasterKey();
            if (key == null) return null;

            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var nonce = new byte[12]; // recommended size for GCM
            RandomNumberGenerator.Fill(nonce);

            var ciphertext = new byte[plaintextBytes.Length];
            var tag = new byte[16]; // 128-bit tag

            using (var aes = new AesGcm(key, 16))
            {
                aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);
            }

            return Combine(nonce, tag, ciphertext);
        }

        // Decrypt raw bytes (nonce|tag|ciphertext) -> plaintext
        public static string? Decrypt(byte[] combined)
        {
            var key = GetMasterKey();
            if (key == null) return null;
            else if (combined.Length < 12 + 16)
            {
                WriteLine("Invalid encrypted data.");
                return null;
            }

            var nonce = new byte[12];
            var tag = new byte[16];
            var ciphertext = new byte[combined.Length - nonce.Length - tag.Length];

            Buffer.BlockCopy(combined, 0, nonce, 0, nonce.Length);
            Buffer.BlockCopy(combined, nonce.Length, tag, 0, tag.Length);
            Buffer.BlockCopy(combined, nonce.Length + tag.Length, ciphertext, 0, ciphertext.Length);

            var plaintextBytes = new byte[ciphertext.Length];
            using (var aes = new AesGcm(key, 16))
            {
                aes.Decrypt(nonce, ciphertext, tag, plaintextBytes);
            }

            return Encoding.UTF8.GetString(plaintextBytes);
        }
    }
}