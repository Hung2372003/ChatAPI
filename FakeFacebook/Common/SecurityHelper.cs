using System.Security.Cryptography;
using System.Text;

namespace FakeFacebook.Common
{
    public static class SecurityHelper
    {
        // Tạo hash và salt cho mật khẩu
        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        // Kiểm tra mật khẩu nhập vào có khớp với hash/salt không
        public static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(storedHash);
            }
        }

        // Mã hóa AES
        public static string EncryptAes(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            using (Aes aesAlg = Aes.Create())
            {
                var keyBytes = Encoding.UTF8.GetBytes(key);
                // Đảm bảo key đủ 32 bytes cho AES-256
                Array.Resize(ref keyBytes, 32);
                aesAlg.Key = keyBytes;
                aesAlg.GenerateIV();
                var iv = aesAlg.IV;
                using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv))
                using (var msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(iv, 0, iv.Length); // prepend IV
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        // Giải mã AES
        public static string DecryptAes(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;
            var fullCipher = Convert.FromBase64String(cipherText);
            using (Aes aesAlg = Aes.Create())
            {
                var keyBytes = Encoding.UTF8.GetBytes(key);
                Array.Resize(ref keyBytes, 32);
                aesAlg.Key = keyBytes;
                var iv = new byte[16];
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                aesAlg.IV = iv;
                using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                using (var msDecrypt = new MemoryStream(fullCipher, 16, fullCipher.Length - 16))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
        // Tạo khóa AES ngẫu nhiên (32 bytes cho AES-256)
        public static string GenerateRandomAesKey()
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                return Convert.ToBase64String(aes.Key);
            }
        }
        // Mã hóa khóa AES bằng RSA Public Key của người dùng
        public static string EncryptWithRsa(string plainText, string publicKey)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(publicKey)) return null;
            
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                // Giả định Public Key lưu dưới dạng XML trong DB
                rsa.FromXmlString(publicKey);
                var data = Encoding.UTF8.GetBytes(plainText);
                var encrypted = rsa.Encrypt(data, false);
                return Convert.ToBase64String(encrypted);
            }
        }

        // Hàm mã hóa tin nhắn (có thể bổ sung sau)
        // public static string EncryptMessage(string message, string key) { ... }
        // public static string DecryptMessage(string encrypted, string key) { ... }
        // Hàm làm sạch input để chống XSS (chỉ giữ lại text, loại bỏ thẻ script, html)
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            // Loại bỏ các thẻ HTML/script đơn giản (cơ bản, có thể thay thế bằng thư viện mạnh hơn nếu cần)
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
        }
    }
}
