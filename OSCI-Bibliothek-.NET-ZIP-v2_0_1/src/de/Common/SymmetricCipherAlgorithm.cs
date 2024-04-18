using System;
using Osci.Extensions;

namespace Osci.Common
{
    public enum SymmetricCipherAlgorithm
    {
        [AlgorithmInfo(XmlName = Constants.SymmetricCipherAlgorithmAes128Gcm)]
        Aes128Gcm,

        [AlgorithmInfo(XmlName = Constants.SymmetricCipherAlgorithmAes192Gcm)]
        Aes192Gcm,

        [AlgorithmInfo(XmlName = Constants.SymmetricCipherAlgorithmAes256Gcm)]
        Aes256Gcm,

        [Obsolete("The algorithm is vulnerable to padding oracle attacks.")]
        [AlgorithmInfo(XmlName = Constants.SymmetricCipherAlgorithmTripleDesCbc)]
        TripleDesCbc,

        [Obsolete("The algorithm is vulnerable to padding oracle attacks.")]
        [AlgorithmInfo(XmlName = Constants.SymmetricCipherAlgorithmAes128Cbc)]
        Aes128Cbc,

        [Obsolete("The algorithm is vulnerable to padding oracle attacks.")]
        [AlgorithmInfo(XmlName = Constants.SymmetricCipherAlgorithmAes192Cbc)]
        Aes192Cbc,

        [Obsolete("The algorithm is vulnerable to padding oracle attacks.")]
        [AlgorithmInfo(XmlName = Constants.SymmetricCipherAlgorithmAes256Cbc)]
        Aes256Cbc
    }

    public static class SymmetricCipherAlgorithmExtensions
    {
        internal static string GetXmlName(this SymmetricCipherAlgorithm e)
        {
            return ((Enum)e).GetXmlName();
        }
    }
}