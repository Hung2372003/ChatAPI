using System.Security.Cryptography;
using System.Text;

namespace FakeFacebook.Common
{
    public class RsaKeyProvider
    {
        public string PrivateKeyPem { get; }
        public string PublicKeyPem { get; }

        public RsaKeyProvider()
        {
            using var rsa = RSA.Create(2048);

            // 🔑 Private key (PKCS#8)
            var privateKeyBytes = rsa.ExportPkcs8PrivateKey();
            PrivateKeyPem = ToPem("PRIVATE KEY", privateKeyBytes);

            // 🔓 Public key (X509 / SubjectPublicKeyInfo)
            var publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();
            PublicKeyPem = ToPem("PUBLIC KEY", publicKeyBytes);
        }

        private static string ToPem(string title, byte[] data)
        {
            var base64 = Convert.ToBase64String(data);
            var sb = new StringBuilder();
            sb.AppendLine($"-----BEGIN {title}-----");

            for (int i = 0; i < base64.Length; i += 64)
                sb.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));

            sb.AppendLine($"-----END {title}-----");
            return sb.ToString();
        }
    }
}
