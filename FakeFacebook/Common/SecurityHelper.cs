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
