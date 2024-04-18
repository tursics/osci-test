using System.Collections.Generic;
using System.Linq;

namespace Osci.Common
{
    internal static class NamespaceMap
    {
        public static string GetXmlNamespace(string key)
        {
            return _bouncyCastleMap.ContainsValue(key) ? _bouncyCastleMap.Single(_ => _.Value.Equals(key)).Key : null;
        }

        public static string GetBouncyCastleName(string key)
        {
            return _bouncyCastleMap.ContainsKey(key) ? _bouncyCastleMap[key] : null;
        }

        private static readonly Dictionary<string, string> _bouncyCastleMap = new Dictionary<string, string>
        {
            { Constants.DigestAlgorithmSha1, "SHA-1" },
            { Constants.DigestAlgorithmSha256, "SHA-256" },
            { Constants.DigestAlgorithmSha512, "SHA-512" },
            { Constants.DigestAlgorithmRipemd160, "RIPEMD160" },
            { Constants.DigestAlgorithmSha3With256Bit, "SHA3-256" },
            { Constants.DigestAlgorithmSha3With384Bit, "SHA3-384" },
            { Constants.DigestAlgorithmSha3With512Bit, "SHA3-512" }
        };


        /*
        
        BOUNCYCASTLE-NAMES 
        ============================
        Digest-Algorithms:
        ------------------
        RIPEMD128
        SHA-256
        MD4
        SHAKE256
        SHA-512/256
        SHA-384
        SHA-1
        MD5
        SHA3-384
        MD2
        SHA-512/224
        SHA3-256
        RIPEMD160
        RIPEMD256
        SHA-224
        SHA-512
        SHA3-512
        SHAKE128
        GOST3411
        SHA3-224

        Signer-Algorithms:
        ------------------
        SHA-512withECDSA
        MD5withRSA
        SHA-224withECDSA
        SHA-512withRSA
        MD2withRSA
        SHA-384withRSA
        SHA-1withECDSA
        SHA-384withRSAandMGF1
        SHA-256withECDSA
        MD4withRSA
        SHA-256withRSAandMGF1
        SHA-1withDSA
        RIPEMD128withRSA
        SHA-256withRSA
        SHA-384withECDSA
        SHA-224withRSA
        SHA-512withRSAandMGF1
        SHA-1withRSAandMGF1
        PSSwithRSA
        GOST3410
        SHA-224withRSAandMGF1
        SHA-1withRSA
        RIPEMD256withRSA
        ECGOST3410
        RIPEMD160withRSA

         */
    }
}