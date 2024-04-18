using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Osci.Helper;

namespace Osci.Extensions
{
    internal static class EcdsaSignerExtensions
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(EcdsaSignerExtensions));


        public static bool VerifySignature(this ECDsaSigner signer, byte[] data, byte[] signature, string digestAlgorithm)
        {
            byte[] digest = DigestUtilities.CalculateDigest(digestAlgorithm, data);

            List<ByteLength> byteLengths = new List<ByteLength> { new ByteLength(signature.Length / 2, signature.Length - signature.Length / 2) };

            if (signature.Length % 2 != 0)
            {
                ByteLength defaultLength = byteLengths.First();

                // since there is no info inside the signature byte array 
                // how to get the r signature value and s signature value,
                // so we need a to guess around the "standard" split point
                byteLengths.Add(new ByteLength(defaultLength.R - 1, defaultLength.S + 1));
                byteLengths.Add(new ByteLength(defaultLength.R + 1, defaultLength.S - 1));
                byteLengths.Add(new ByteLength(defaultLength.R - 2, defaultLength.S + 2));
                byteLengths.Add(new ByteLength(defaultLength.R + 2, defaultLength.S - 2));
            }

            foreach (ByteLength byteLength in byteLengths)
            {
                BigInteger r = new BigInteger(1, signature, 0, byteLength.R);
                BigInteger s = new BigInteger(1, signature, byteLength.R, byteLength.S);

                _log.Debug("Signature.Length: " + signature.Length);
                _log.Debug("r: " + r.BitLength);
                _log.Debug("s: " + s.BitLength);

                bool result = signer.VerifySignature(digest, r, s);
                if (result)
                {
                    return true;
                }
            }

            return false;
        }


        private class ByteLength
        {
            public int R
            {
                get; private set;
            }
            public int S
            {
                get; private set;
            }

            public ByteLength(int r, int s)
            {
                R = r;
                S = s;
            }
        }
    }
}
