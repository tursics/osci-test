using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Osci.Helper;

namespace Osci.Extensions
{
    public static class X509CertificateExtensions
    {
        public static int GetKeySize(this X509Certificate certificate)
        {
            AsymmetricKeyParameter asymmetricKeyParameter = certificate.GetPublicKey();
            ECPublicKeyParameters parameters = asymmetricKeyParameter as ECPublicKeyParameters;
            return parameters != null ? parameters.Parameters.N.BitLength : ((RsaKeyParameters)asymmetricKeyParameter).Modulus.BitLength;
        }
    }
}
