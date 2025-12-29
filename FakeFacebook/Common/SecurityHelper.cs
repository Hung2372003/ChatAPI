using System.Security.Cryptography;
using System.Text;

namespace FakeFacebook.Common
{
    public static class SecurityHelper
    {
        // ================= AES =================

        // 🔐 Encrypt → Base64(IV + Cipher)
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

            // 📦 Gộp IV + Cipher
            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        // 🔓 Decrypt Base64(IV + Cipher)
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

        // ================= RSA =================

        // 🔐 RSA decrypt AES key (OAEP SHA256)
        public static string DecryptRsa(string base64Cipher, string privateKeyXml)
        {
            var cipherBytes = Convert.FromBase64String(base64Cipher);

            using var rsa = RSA.Create();
            rsa.FromXmlString(privateKeyXml);

            var decrypted = rsa.Decrypt(cipherBytes, RSAEncryptionPadding.OaepSHA1);
            return Encoding.UTF8.GetString(decrypted);
        }

        // ================= RSA KEY =================

        public static (string privateKeyXml, string publicKeyXml) GenerateRsaKeyPair(int keySize = 2048)
        {
            using var rsa = RSA.Create(keySize);
            return (rsa.ToXmlString(true), rsa.ToXmlString(false));
        }

        // ================= PASSWORD =================

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

        public static string GenerateRandomAesKey()
        {
            // 256 bit = 32 bytes
            var key = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(key);
        }
        public static string EncryptWithRsa(string plainText, string publicKeyXml)
        {
            var data = Encoding.UTF8.GetBytes(plainText);

            using var rsa = RSA.Create();
            rsa.FromXmlString(publicKeyXml);

            var encrypted = rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA1);
            return Convert.ToBase64String(encrypted);
        }
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            #if HAS_HTMLSANITIZER
                        // Nếu đã cài HtmlSanitizer (Ganss.XSS), dùng thư viện này
                        var sanitizer = new Ganss.XSS.HtmlSanitizer();
                        return sanitizer.Sanitize(input);
            #else
            // Nếu chưa cài, fallback về hàm cũ (loại bỏ thẻ html/script đơn giản)
            var array = new char[input.Length];
            int arrayIndex = 0;
            bool inside = false;
            foreach (var @let in input)
            {
                if (@let == '<') { inside = true; continue; }
                if (@let == '>') { inside = false; continue; }
                if (!inside) { array[arrayIndex] = @let; arrayIndex++; }
            }
            return new string(array, 0, arrayIndex);
            #endif
        }
    }
}
