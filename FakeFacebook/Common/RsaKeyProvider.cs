using System.Security.Cryptography;

namespace FakeFacebook.Common
{

    public class RsaKeyProvider
    {
        public string PrivateKeyXml { get; }
        public string PublicKeyXml { get; }

        public RsaKeyProvider()
        {
            using var rsa = RSA.Create(2048);
            PrivateKeyXml = rsa.ToXmlString(true);   // private + public
            PublicKeyXml = rsa.ToXmlString(false);  // public only
        }
    }
}
