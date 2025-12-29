using System.Security.Cryptography;
using System.Text;

namespace FakeFacebook.Common
{
    public static class SecurityHelper
    {
        // =====================================================
        // ====================== AES ==========================
        // =====================================================

        // 🔐 Encrypt: Base64( IV + Cipher )
        public static string EncryptAes(string plainText, string aesKeyBase64)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            var key = Convert.FromBase64String(aesKeyBase64);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // IV + Cipher
            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        // 🔓 Decrypt: Base64( IV + Cipher )
        public static string DecryptAes(string base64Data, string aesKeyBase64)
        {
            if (string.IsNullOrEmpty(base64Data)) return base64Data;

            var fullCipher = Convert.FromBase64String(base64Data);

            var iv = new byte[16];
            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);

            var cipherText = new byte[fullCipher.Length - iv.Length];
            Buffer.BlockCopy(fullCipher, iv.Length, cipherText, 0, cipherText.Length);

            var key = Convert.FromBase64String(aesKeyBase64);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }

        // 🔑 Generate random AES key (256 bit)
        public static string GenerateRandomAesKey()
        {
            var key = RandomNumberGenerator.GetBytes(32); // 256 bit
            return Convert.ToBase64String(key);
        }

        // =====================================================
        // ====================== RSA ==========================
        // =====================================================

        // 🔐 RSA Encrypt (PEM, OAEP SHA256)
        public static string EncryptWithRsaPem(string plainText, string publicKeyPem)
        {
            var data = Encoding.UTF8.GetBytes(plainText);

            using var rsa = RSA.Create();

            var keyBase64 = publicKeyPem
                .Replace("-----BEGIN PUBLIC KEY-----", "")
                .Replace("-----END PUBLIC KEY-----", "")
                .Replace("\r", "")
                .Replace("\n", "");

            var keyBytes = Convert.FromBase64String(keyBase64);
            rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);

            var encrypted = rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA1);
            return Convert.ToBase64String(encrypted);
        }

        // 🔓 RSA Decrypt (PEM, OAEP SHA256)
        public static string DecryptRsaPem(string base64Cipher, string privateKeyPem)
        {
            var cipherBytes = Convert.FromBase64String(base64Cipher);

            using var rsa = RSA.Create();

            var keyBase64 = privateKeyPem
                .Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "")
                .Replace("\r", "")
                .Replace("\n", "");

            var keyBytes = Convert.FromBase64String(keyBase64);
            rsa.ImportPkcs8PrivateKey(keyBytes, out _);

            var decrypted = rsa.Decrypt(cipherBytes, RSAEncryptionPadding.OaepSHA1);
            return Encoding.UTF8.GetString(decrypted);
        }

        // 🔑 Generate RSA key pair (PEM)
        public static (string privatePem, string publicPem) GenerateRsaKeyPairPem()
        {
            using var rsa = RSA.Create(2048);

            var privateKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
            var publicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());

            return (
                $"-----BEGIN PRIVATE KEY-----\n{privateKey}\n-----END PRIVATE KEY-----",
                $"-----BEGIN PUBLIC KEY-----\n{publicKey}\n-----END PUBLIC KEY-----"
            );
        }

        // =====================================================
        // =================== PASSWORD ========================
        // =====================================================

        public static void CreatePasswordHash(
            string password,
            out byte[] passwordHash,
            out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        public static bool VerifyPasswordHash(
            string password,
            byte[] storedHash,
            byte[] storedSalt)
        {
            using var hmac = new HMACSHA512(storedSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(storedHash);
        }

        // =====================================================
        // ==================== SANITIZE =======================
        // =====================================================

        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var array = new char[input.Length];
            int arrayIndex = 0;
            bool inside = false;

            foreach (var c in input)
            {
                if (c == '<') { inside = true; continue; }
                if (c == '>') { inside = false; continue; }
                if (!inside)
                {
                    array[arrayIndex] = c;
                    arrayIndex++;
                }
            }

            return new string(array, 0, arrayIndex);
        }
    }
}
